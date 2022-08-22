using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using KModkit;
using RemoteMath.Actions;
using UnityEngine;
using UnityEngine.Serialization;

namespace RemoteMath
{
    public class RemoteMathScript : MonoBehaviour
    {
        [FormerlySerializedAs("BombAudio")] public KMAudio bombAudio;
        [FormerlySerializedAs("BombInfo")] public KMBombInfo bombInfo;
        [FormerlySerializedAs("BombModule")] public KMBombModule bombModule;
        [FormerlySerializedAs("ModuleSelect")] public KMSelectable moduleSelect;
        [FormerlySerializedAs("MainButton")] public KMSelectable mainButton;

        [FormerlySerializedAs("SecretCodeText")]
        public GameObject secretCodeText;

        [FormerlySerializedAs("WelcomeText")] public GameObject welcomeText;
        public GameObject fakeStatusLitBoi;
        public GameObject realStatusLitBoi;
        [FormerlySerializedAs("Fruit1")] public GameObject fruit1;
        [FormerlySerializedAs("Fruit2")] public GameObject fruit2;
        [FormerlySerializedAs("FruitMats")] public Material[] fruitMats;
        [FormerlySerializedAs("Lights")] public Light[] lights;
        [FormerlySerializedAs("FruitNames")] public string[] fruitNames;
        private RemoteMathNet _remoteMathApi;
        private string _currentLed;

        private bool _moduleSolved;
        private bool _allowedToSolve;
        private bool _moduleStartup;
        private bool _hasErrored;
        private string _secretToken;
        private byte _disconnectCount;

        private bool _twitchPlaysMode;
#pragma warning disable CS0649
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once ArrangeTypeMemberModifiers
        bool TwitchPlaysActive;
#pragma warning restore CS0649

        private bool TwitchPlaysMode
        {
            get { return TwitchPlaysActive || _twitchPlaysMode; }
            set { _twitchPlaysMode = value; }
        }

        private readonly List<string> _twitchPlaysCodes = new List<string>();

        private static int _moduleIdCounter = 1;
        private int _moduleId;
        private string _twitchId;

        private void GetTwitchPlaysId()
        {
            var gType = ReflectionHelper.FindType("TwitchGame", "TwitchPlaysAssembly");
            object comp = FindObjectOfType(gType);
            if (comp == null) return;
            var twitchModules = comp.GetType().GetField("Modules", BindingFlags.Public | BindingFlags.Instance);
            if (twitchModules == null) return;
            var twitchPlaysObj = twitchModules.GetValue(comp);
            var twitchPlaysModules = (IEnumerable) twitchPlaysObj;
            foreach (var module in twitchPlaysModules)
            {
                var bombComponent = module.GetType().GetField("BombComponent", BindingFlags.Public | BindingFlags.Instance);
                if (bombComponent == null) continue;
                var behaviour = (MonoBehaviour) bombComponent.GetValue(module);
                var rMath = behaviour.GetComponent<RemoteMathScript>();
                if (rMath != this) continue;
                var moduleCode = module.GetType().GetProperty("Code", BindingFlags.Public | BindingFlags.Instance);
                if (moduleCode != null) _twitchId = (string) moduleCode.GetValue(module, null);
            }
        }

        private void Start()
        {
            _moduleId = _moduleIdCounter++;

            var scalar = transform.lossyScale.x;
            foreach (var t in lights)
                t.range *= scalar;

            SetSecretCode("");
            SetLed("Off");

            mainButton.OnInteract += delegate
            {
                if (!_moduleStartup)
                {
                    secretCodeText.SetActive(true);
                    welcomeText.SetActive(false);
                    _moduleStartup = true;
                    StartCoroutine(StartUdpClient());
                    return false;
                }

                if (_moduleSolved) return false;
                if (!_allowedToSolve) return false;
                _moduleSolved = true;
                HandlePass();
                return false;
            };

            fruit1.SetActive(false);
            fruit2.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_moduleStartup && _remoteMathApi.IsAlive()) _remoteMathApi.Close();
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private IEnumerator StartUdpClient()
        {
            _remoteMathApi = new RemoteMathNet();
            _remoteMathApi.Message += NetMessage;
            _remoteMathApi.Disconnect += NetDisconnected;
            _remoteMathApi.Error += NetError;
            _remoteMathApi.Connect(_secretToken);
            Debug.LogFormat("[Remote Math #{0}] Connected", _moduleId);
            SetLed("White");
            yield return WaitForWebsocketTimeout();
            yield return null;
        }

        private void NetDisconnected(object sender, EventArgs e)
        {
            if (_allowedToSolve) return;
            if (_remoteMathApi.IsClosing())
            {
                Debug.LogFormat("[Remote Math #{0}] Connection disconnected for module destruction",_moduleId);
                return;
            }
            Debug.LogFormat("[Remote Math #{0}] Connection disconnected... attempting reconnect", _moduleId);
            // ReSharper disable once StringLiteralTypo
            SetSecretCode("NETDCN");
            SetLed("Purple");
            if (_secretToken != "")
            {
                // If there is more than 3 disconnects then error
                _disconnectCount++;
                if (_disconnectCount > 3) NetError(sender, e);
                else _remoteMathApi.Connect(_secretToken);
            }
            else NetError(sender, e);
        }

        private void NetError(object sender, EventArgs e)
        {
            if (_allowedToSolve) return;
            Debug.LogFormat("[Remote Math #{0}] Connection error... auto solve mode", _moduleId);
            // ReSharper disable once StringLiteralTypo
            SetSecretCode("NETERR");
            SetLed("Blue");
            _hasErrored = true;
            TriggerModuleSolve();
        }

        private void NetMessage(object sender, RemoteMathNet.MessageEventArgs e)
        {
            switch (e.Message.Action)
            {
                case ActionByte.PuzzleCreate:
                    break;
                case ActionByte.PuzzleInvalid:
                    break;
                case ActionByte.PuzzleToken:
                    ActionParse.PuzzleToken(e.Message.Data, out _secretToken);
                    Debug.LogFormat("[Remote Math #{0}] Puzzle token: {1}", _moduleId, _secretToken);
                    break;
                case ActionByte.PuzzleLog:
                    string logCode;
                    ActionParse.PuzzleLog(e.Message.Data, out logCode);
                    Debug.LogFormat("[Remote Math #{0}] Log URL: {1}", _moduleId, logCode);
                    break;
                case ActionByte.PuzzleCode:
                    string code;
                    ActionParse.PuzzleCode(e.Message.Data, out code);
                    Debug.LogFormat("[Remote Math #{0}] Puzzle Code: {1}", _moduleId, code);
                    var dispatcher = UnityMainThreadDispatcher.Instance();
                    dispatcher.Enqueue(SendPuzzleFruit());
                    dispatcher.Enqueue(SendBombDetails());
                    SetSecretCode(code);
                    break;
                case ActionByte.PuzzleSolve:
                    Debug.LogFormat("[Remote Math #{0}] Puzzle Completed", _moduleId);
                    SetSecretCode("DONE", true);
                    SetLed("Orange");
                    _remoteMathApi.Close();
                    TriggerModuleSolve();
                    break;
                case ActionByte.PuzzleStrike:
                    Debug.LogFormat("[Remote Math #{0}] Puzzle Strike", _moduleId);
                    HandleStrike();
                    break;
                case ActionByte.PuzzleTwitchCode:
                    string twitchCode;
                    ActionParse.PuzzleTwitchCode(e.Message.Data, out twitchCode);
                    _twitchPlaysCodes.Add(twitchCode);
                    break;
            }
        }

        private IEnumerator SendPuzzleFruit()
        {
            var fruitNumbers = new List<byte>();
            for (var i = 0; i < 8; i++) fruitNumbers.Add((byte) UnityEngine.Random.Range(0f, fruitMats.Length));

            fruit1.transform.Find("FruitImage").gameObject.GetComponent<MeshRenderer>().material = fruitMats[fruitNumbers[0]];
            fruit2.transform.Find("FruitImage").gameObject.GetComponent<MeshRenderer>().material = fruitMats[fruitNumbers[1]];
            fruit1.transform.Find("FruitText").gameObject.GetComponent<TextMesh>().text = fruitNames[fruitNumbers[2]];
            fruit2.transform.Find("FruitText").gameObject.GetComponent<TextMesh>().text = fruitNames[fruitNumbers[3]];
            fruit1.SetActive(true);
            fruit2.SetActive(true);
            Debug.LogFormat("[Remote Math #{0}] Puzzle Fruits: {1}", _moduleId, fruitNumbers.ToArray().Join(", "));
            _remoteMathApi.Send(ActionFactory.PuzzleFruits(fruitNumbers.ToArray()));
            yield return null;
        }

        private IEnumerator SendBombDetails()
        {
            var batteryCount = bombInfo.GetBatteryCount();
            var portCount = bombInfo.GetPortCount();
            Debug.LogFormat("[Remote Math #{0}] Battery Count: {1}, Port Count: {2}", _moduleId, batteryCount, portCount);
            _remoteMathApi.Send(ActionFactory.BombDetails(batteryCount, portCount));
            yield return null;
        }

        private void TriggerModuleSolve()
        {
            _allowedToSolve = true;
        }

        private void SetLed(string led)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(ShowLed(led));
        }

        private void SetSecretCode(string code)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(ShowSecretCode(code, false));
        }

        private void SetSecretCode(string code, bool ignoreLineBreak)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(ShowSecretCode(code, ignoreLineBreak));
        }

        private IEnumerator ShowSecretCode(string code, bool ignoreLineBreak)
        {
            secretCodeText.GetComponent<TextMesh>().text = ignoreLineBreak || code.Length < 4 ? code : code.Substring(0, 4) + "\n" + code.Substring(4);
            yield return null;
        }

        private IEnumerator WaitForWebsocketTimeout()
        {
            yield return new WaitForSeconds(15f);
            if (_remoteMathApi.IsAlive()) yield break;
            Debug.LogFormat("[Remote Math #{0}] Connection Failed", _moduleId);
            SetLed("Blue");
            _remoteMathApi.Close();
            TriggerModuleSolve();
        }

        private IEnumerator ShowLed(string led)
        {
            _currentLed = led;
            realStatusLitBoi.SetActive(false);
            var transformForFakeStatusLitBoi = fakeStatusLitBoi.transform;
            for (var i = 0; i < transformForFakeStatusLitBoi.childCount; i++) transformForFakeStatusLitBoi.GetChild(i).gameObject.SetActive(false);

            if (led != "Off") transformForFakeStatusLitBoi.Find(led).gameObject.SetActive(true);
            else realStatusLitBoi.SetActive(true);
            yield return null;
        }

        private void HandlePass()
        {
            UnityMainThreadDispatcher.Instance().Enqueue(HandlePassEnumerator());
        }

        private void HandleStrike()
        {
            UnityMainThreadDispatcher.Instance().Enqueue(HandleStrikeEnumerator());
        }

        private IEnumerator HandlePassEnumerator()
        {
            bombModule.HandlePass();
            SetLed("Green");
            yield return null;
        }

        private IEnumerator HandleStrikeEnumerator()
        {
            bombModule.HandleStrike();
            var savedLed = _currentLed;
            SetLed("Red");
            yield return new WaitForSeconds(1.5f);
            SetLed(savedLed);
            yield return null;
        }

#pragma warning disable 414
        // ReSharper disable once InconsistentNaming
        private readonly string TwitchHelpMessage = @"Use `!{0} go` to start the module and then use it again once you have solved it.";
#pragma warning restore 414

        // ReSharper disable once UnusedMember.Local
        private IEnumerator ProcessTwitchCommand(string command)
        {
            command = command.ToLowerInvariant().Trim();
            Debug.Log(command);
            if (command == "go")
            {
                yield return null;
                TwitchPlaysMode = true;
                GetTwitchPlaysId();
                if (!_hasErrored && _allowedToSolve)
                    // ReSharper disable once StringLiteralTypo
                    yield return "awardpointsonsolve -8";
                mainButton.OnInteract();
            }
            else if (Regex.IsMatch(command, @"^check +[0-9]{3}$"))
            {
                if (!TwitchPlaysMode) yield break;
                var vs = command.Split(' ');
                var code = vs.TakeLast(1).Join();
                yield return null;
                if (_twitchPlaysCodes.Contains(code))
                {
                    _remoteMathApi.Send(ActionFactory.PuzzleTwitchConfirmCode(code));
                    // ReSharper disable once StringLiteralTypo
                    yield return "sendtochat The requested expert module for Remote Math {1} has been activated";
                    yield return "strike";
                    yield return "solve";
                }
                else
                {
                    // ReSharper disable once StringLiteralTypo
                    yield return "sendtochat The requested expert module for Remote Math {1} doesn't exist";
                }
            }
        }
    }
}