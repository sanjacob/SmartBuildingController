using NUnit.Framework;
using System.Reflection;
using SmartBuilding;
using System.Globalization;
using NSubstitute;

namespace BuildingControllerTests
{
    /// <summary>
    /// Test Fixture for the class <see cref="BuildingController"/>.
    /// </summary>
    [TestFixture]
    [Author("Jacob Sanchez", "jsanchez-perez@uclan.ac.uk")]
    public class BuildingControllerTests
    {
        /// <summary>
        /// Valid states for the <see cref="BuildingController"/>
        /// </summary>
        struct BuildingState
        {
            public const string closed = "closed";
            public const string outOfHours = "out of hours";
            public const string open = "open";
            public const string fireDrill = "fire drill";
            public const string fireAlarm = "fire alarm";
        }

        /// <summary>
        /// Argument names for the <see cref="BuildingController"/> constructor.
        /// </summary>
        struct ControllerArgNames
        {
            public const string buildingID = "id";
            public const string startState = "startState";
            public const string lightManager = "iLightManager";
            public const string fireAlarmManager = "iFireAlarmManager";
            public const string doorManager = "iDoorManager";
            public const string webService = "iWebService";
            public const string emailService = "iEmailService";
        }

        /// <summary>
        /// Store expected strings for <see cref="BuildingController"/> tests.
        /// </summary>
        struct ExpectedStrings
        {
            public const string initialStateException = "Argument Exception: BuildingController can only be initialised "
                + "to the following states 'open', 'closed', 'out of hours'";
            public const string emailSubject = "failed to log alarm";
            public const string emailAddress = "smartbuilding@uclan.ac.uk";
        }

        /// <summary>
        /// Testing strings for managers.
        /// </summary>
        struct ManagerStatus
        {
            public const string lights = "Lights";
            public const string doors = "Doors";
            public const string alarm = "FireAlarm";

            public const string lightsPrefix = lights + ",";
            public const string doorsPrefix = doors + ",";
            public const string alarmPrefix = alarm + ",";

            public const string singleDeviceOk = "OK,";
            public const string singleDeviceFault = "FAULT,";
            public const string threeDevicesOk = "OK,OK,OK,";
            public const string threeDevicesMixed = "OK,FAULT,OK,";
            public const string fiveDevicesOk = "OK,OK,OK,OK,OK,";
            public const string tenDevicesOk = "OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,";
            public const string twelveDevicesOk = "OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,";
            public const string tenDevicesMixed = "FAULT,OK,OK,OK,OK,OK,OK,OK,OK,OK,";
            public const string tenDevicesFault = "FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,";
            public const string manyDevicesOk ="OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK," +
                "OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,";
            public const string manyDevicesMixed = "OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK," +
                "OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,FAULT,";
            public const string manyDevicesFault = "FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT," +
                "FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT," +
                "FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT," +
                "FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,";
        }

        private static readonly object[] ValidBuildingStates =
        {
            BuildingState.closed,
            BuildingState.outOfHours,
            BuildingState.open,
            BuildingState.fireAlarm,
            BuildingState.fireDrill
        };

        private static readonly object[] NormalBuildingStates =
        {
            BuildingState.closed,
            BuildingState.outOfHours,
            BuildingState.open
        };

        private static readonly object[] InvalidBuildingStates =
        {
            "out of service",
            "invalid"
        };

        /// <summary>
        /// Array containing a variety of strings to test against.
        /// </summary>
        private static readonly object?[] TestStrings =
        {
            null,
            "",
            "null",
            "abcdefghijklmnopqrstuvwxyz",
            "01234567890",
            "The quick fox jumps over the lazy dog",
            "á, é, í, ó, ú",
            "🥸",
            "'",
            ",",
            "\"",
            "!@#$%^&*(){}?+_:;/=-]['",
            "   ",
            "\n",
            "\r",
            "\x05d\U000507a7",
            "0",
            "Ý",
            "\U000b2d04\x87\U0007e552A",
            "©þ\nù",
            "ëõð",
            "\x13",
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit, " +
            "sed do eiusmod tempor incididunt ut labore et dolore magna " +
            "aliqua. Ut enim ad minim veniam, quis nostrud exercitation " +
            "ullamco laboris nisi ut aliquip ex ea commodo consequat. " +
            "Duis aute irure dolor in reprehenderit in voluptate velit " +
            "esse cillum dolore eu fugiat nulla pariatur. Excepteur sint " +
            "occaecat cupidatat non proident, sunt in culpa qui officia " +
            "deserunt mollit anim id est laborum."
        };

        private static readonly object?[] OkManagerStatuses =
        {
            "",
            ManagerStatus.singleDeviceOk,
            ManagerStatus.threeDevicesOk,
            ManagerStatus.tenDevicesOk,
            ManagerStatus.twelveDevicesOk,
            ManagerStatus.manyDevicesOk,
        };

        private static readonly object?[] FaultyManagerStatuses =
        {
            ManagerStatus.singleDeviceFault,
            ManagerStatus.threeDevicesMixed,
            ManagerStatus.tenDevicesFault,
            ManagerStatus.tenDevicesMixed,
            ManagerStatus.manyDevicesMixed,
            ManagerStatus.manyDevicesFault
        };

        /// <summary>
        /// Example return values for <see cref="ILightManager"/> stubs.
        /// </summary>
        private static readonly object?[] LightManagerStatuses =
        {
            ManagerStatus.lightsPrefix,
            ManagerStatus.lightsPrefix + ManagerStatus.threeDevicesMixed,
            ManagerStatus.lightsPrefix + ManagerStatus.tenDevicesOk,
            ManagerStatus.lightsPrefix + ManagerStatus.tenDevicesFault,
            ManagerStatus.lightsPrefix + ManagerStatus.manyDevicesOk,
        };

        /// <summary>
        /// Example return values for <see cref="IDoorManager"/> stubs.
        /// </summary>
        private static readonly object?[] DoorManagerStatuses =
        {
            ManagerStatus.doorsPrefix,
            ManagerStatus.doorsPrefix + ManagerStatus.threeDevicesMixed,
            ManagerStatus.doorsPrefix + ManagerStatus.tenDevicesOk,
            ManagerStatus.doorsPrefix + ManagerStatus.tenDevicesFault,
            ManagerStatus.doorsPrefix + ManagerStatus.manyDevicesOk,
        };

        /// <summary>
        /// Example return values for <see cref="IFireAlarmManager"/> stubs.
        /// </summary>
        private static readonly object?[] AlarmManagerStatuses =
        {
            ManagerStatus.alarmPrefix,
            ManagerStatus.alarmPrefix + ManagerStatus.threeDevicesMixed,
            ManagerStatus.alarmPrefix + ManagerStatus.tenDevicesOk,
            ManagerStatus.alarmPrefix + ManagerStatus.tenDevicesFault,
            ManagerStatus.alarmPrefix + ManagerStatus.manyDevicesOk,
        };


        // LEVEL 1 TESTS //

        /// <summary>
        /// Test that a valid constructor exists for <see cref="BuildingController"/> through reflection.
        /// Satisfies <strong>L1R1</strong>.
        /// </summary>
        [Test]
        public void Constructor_WhenSingleParameter_HasCorrectSignature()
        {
            string? parameterName = null;
            ConstructorInfo? constructorInfoObj;
            Type[] argTypes = new Type[] { typeof(string) };

            // Lookup constructor with specified parameter
            constructorInfoObj = typeof(BuildingController).GetConstructor(argTypes);
            Assume.That(constructorInfoObj, Is.Not.Null);

            if (constructorInfoObj != null)
            {
                // Verify parameter name
                ParameterInfo[] constructorParams = constructorInfoObj.GetParameters();
                ParameterInfo firstParam = constructorParams.First();
                parameterName = firstParam.Name;
            }

            Assert.That(parameterName, Is.EqualTo(ControllerArgNames.buildingID));
        }

        /// <summary>
        /// Test initialisation of <c>buildingID</c> when constructor parameter set.
        /// Satisfies <strong>L1R2</strong>, <strong>L1R3</strong>.
        /// </summary>
        [TestCase("Building ID")]
        [TestCaseSource(nameof(TestStrings))]
        public void Constructor_WhenSet_InitialisesBuildingID(string buildingID)
        {
            BuildingController controller;

            controller = new BuildingController(buildingID);
            string result = controller.GetBuildingID();


            string expected = buildingID;
            if (!string.IsNullOrEmpty(buildingID))
            {
                expected = expected.ToLower();
            }
            
            Assert.That(result, Is.EqualTo(expected));
        }

        /// <summary>
        /// Test <c>buildingID</c> setter.
        /// Satisfies <strong>L1R4</strong>.
        /// </summary>
        [TestCase("Building ID")]
        [TestCaseSource(nameof(TestStrings))]
        public void SetBuildingID_WhenSet_SetsID(string buildingID)
        {
            BuildingController controller = new("");

            controller.SetBuildingID(buildingID);
            string result = controller.GetBuildingID();

            string expected = buildingID;
            if (!string.IsNullOrEmpty(buildingID))
            {
                expected = expected.ToLower();
            }

            Assert.That(result, Is.EqualTo(expected));
        }

        /// <summary>
        /// Test default initialisation of <c>currentState</c>.
        /// Satisfies <strong>L1R5</strong>, <strong>L1R6</strong>.
        /// </summary>
        [Test]
        public void Constructor_ByDefault_InitialisesCurrentState()
        {
            BuildingController controller;

            controller = new BuildingController("");
            string result = controller.GetCurrentState();

            Assert.That(result, Is.EqualTo(BuildingState.outOfHours));
        }

        // L1R7 (SetCurrentState from initial state)

        /// <summary>
        /// Test <see cref="BuildingController.SetCurrentState"/> with valid states.
        /// Satisfies <strong>L1R7</strong>.
        /// </summary>
        [TestCaseSource(nameof(ValidBuildingStates))]
        public void SetCurrentState_WhenValidState_ReturnsTrue(string state)
        {
            BuildingController controller = new("");

            // From initial state to any given state
            bool result = controller.SetCurrentState(state);

            Assert.That(result, Is.True);
        }

        /// <summary>
        /// Test <see cref="BuildingController.SetCurrentState"/> with valid states.
        /// Satisfies <strong>L1R7</strong>.
        /// </summary>
        [TestCaseSource(nameof(ValidBuildingStates))]
        public void SetCurrentState_WhenValidState_SetsState(string state)
        {
            BuildingController controller = new("");

            controller.SetCurrentState(state);
            string result = controller.GetCurrentState();

            Assert.That(result, Is.EqualTo(state));
        }

        /// <summary>
        /// Test <see cref="BuildingController.SetCurrentState"/> with invalid states.
        /// Satisfies <strong>L1R7</strong>.
        /// </summary>
        [Test]
        public void SetCurrentState_WhenInvalidState_ReturnsFalse(
            [ValueSource(nameof(InvalidBuildingStates))] [ValueSource(nameof(TestStrings))] string state,
            [ValueSource(nameof(ValidBuildingStates))] string sourceState)
        {
            BuildingController controller = new("");

            controller.SetCurrentState(sourceState);
            bool result = controller.SetCurrentState(state);

            Assert.That(result, Is.False);
        }

        /// <summary>
        /// Test <see cref="BuildingController.SetCurrentState"/> with invalid states.
        /// Satisfies <strong>L1R7</strong>.
        /// </summary>
        [Test]
        public void SetCurrentState_WhenInvalidState_DoesNotSetState(
            [ValueSource(nameof(InvalidBuildingStates))] [ValueSource(nameof(TestStrings))] string state,
            [ValueSource(nameof(ValidBuildingStates))] string sourceState)
        {
            BuildingController controller = new("");

            controller.SetCurrentState(sourceState);
            controller.SetCurrentState(state);
            string result = controller.GetCurrentState();

            Assert.That(result, Is.EqualTo(sourceState));
        }


        // LEVEL 2 TESTS //

        // L2R1 (STD)

        // From Normal States

        /// <summary>
        /// Test <see cref="BuildingController.SetCurrentState"/> when transitioning from 'closed' state.
        /// Satisfies <strong>L2R1</strong>.
        /// </summary>
        [TestCase(BuildingState.outOfHours)]
        [TestCase(BuildingState.fireDrill)]
        [TestCase(BuildingState.fireAlarm)]
        [TestCase(BuildingState.open, false)]
        public void SetCurrentState_WhenCurrentStateClosed_ReturnsBoolean(string state, bool success = true)
        {
            BuildingController controller = new("");

            controller.SetCurrentState(BuildingState.closed);
            bool result = controller.SetCurrentState(state);

            Assert.That(result, Is.EqualTo(success));
        }

        /// <summary>
        /// Test <see cref="BuildingController.SetCurrentState"/> when transitioning from 'open' state.
        /// Satisfies <strong>L2R1</strong>.
        /// </summary>
        [TestCase(BuildingState.outOfHours)]
        [TestCase(BuildingState.fireDrill)]
        [TestCase(BuildingState.fireAlarm)]
        [TestCase(BuildingState.closed, false)]
        public void SetCurrentState_WhenCurrentStateOpen_ReturnsBoolean(string state, bool success = true)
        {
            BuildingController controller = new("");

            controller.SetCurrentState(BuildingState.open);
            bool result = controller.SetCurrentState(state);

            Assert.That(result, Is.EqualTo(success));
        }

        /// <summary>
        /// Test <see cref="BuildingController.SetCurrentState"/> when transitioning from 'open' state.
        /// Satisfies <strong>L2R1</strong>.
        /// </summary>
        [TestCase(BuildingState.outOfHours)]
        [TestCase(BuildingState.fireDrill)]
        [TestCase(BuildingState.fireAlarm)]
        public void SetCurrentState_WhenCurrentStateOpen_SetsState(string state)
        {
            BuildingController controller = new("");

            controller.SetCurrentState(BuildingState.open);
            controller.SetCurrentState(state);
            string result = controller.GetCurrentState();

            Assert.That(result, Is.EqualTo(state));
        }

        /// <summary>
        /// Test <see cref="BuildingController.SetCurrentState"/> when transitioning from 'closed' state.
        /// Satisfies <strong>L2R1</strong>.
        /// </summary>
        [TestCase(BuildingState.outOfHours)]
        [TestCase(BuildingState.fireDrill)]
        [TestCase(BuildingState.fireAlarm)]
        public void SetCurrentState_WhenCurrentStateClosed_SetsState(string state)
        {
            BuildingController controller = new("");

            controller.SetCurrentState(BuildingState.closed);
            controller.SetCurrentState(state);
            string result = controller.GetCurrentState();

            Assert.That(result, Is.EqualTo(state));
        }

        /// <summary>
        /// Test <see cref="BuildingController.SetCurrentState"/> when transitioning from 'closed' state to 'open'.
        /// Satisfies <strong>L2R1</strong>.
        /// </summary>
        [TestCase(BuildingState.open)]
        public void SetCurrentState_WhenCurrentStateClosed_DoesNotSetState(string state)
        {
            BuildingController controller = new("");

            controller.SetCurrentState(BuildingState.closed);
            controller.SetCurrentState(state);
            string result = controller.GetCurrentState();

            Assert.That(result, Is.EqualTo(BuildingState.closed));
        }

        /// <summary>
        /// Test <see cref="BuildingController.SetCurrentState"/> when transitioning from 'open' state to 'closed'.
        /// Satisfies <strong>L2R1</strong>.
        /// </summary>
        [TestCase(BuildingState.closed)]
        public void SetCurrentState_WhenCurrentStateOpen_DoesNotSetState(string state)
        {
            BuildingController controller = new("");

            controller.SetCurrentState(BuildingState.open);
            controller.SetCurrentState(state);
            string result = controller.GetCurrentState();

            Assert.That(result, Is.EqualTo(BuildingState.open));
        }

        // Emergency States

        /// <summary>
        /// Test <see cref="BuildingController.SetCurrentState"/> when transitioning from a 'fire alarm' state to the previous one.
        /// Satisfies <strong>L2R1</strong>.
        /// </summary>
        [TestCaseSource(nameof(NormalBuildingStates))]
        public void SetCurrentState_WhenMovingFromAlarmToPrevious_ReturnsTrue(string state)
        {
            BuildingController controller = new("");

            controller.SetCurrentState(state);
            controller.SetCurrentState(BuildingState.fireAlarm);
            bool result = controller.SetCurrentState(state);
            string newState = controller.GetCurrentState();

            Assert.That(result, Is.True);
            Assert.That(newState, Is.EqualTo(state));
        }

        /// <summary>
        /// Test <see cref="BuildingController.SetCurrentState"/> when transitioning from a 'fire drill' state to the previous one.
        /// Satisfies <strong>L2R1</strong>.
        /// </summary>
        [TestCaseSource(nameof(NormalBuildingStates))]
        public void SetCurrentState_WhenMovingFromDrillToPrevious_ReturnsTrue(string state)
        {
            BuildingController controller = new("");

            controller.SetCurrentState(state);
            controller.SetCurrentState(BuildingState.fireDrill);
            bool result = controller.SetCurrentState(state);
            string newState = controller.GetCurrentState();

            Assert.That(result, Is.True);
            Assert.That(newState, Is.EqualTo(state));
        }

        /// <summary>
        /// Test <see cref="BuildingController.SetCurrentState"/> when transitioning from a 'fire alarm' state to one different from the previous.
        /// Satisfies <strong>L2R1</strong>.
        /// </summary>
        [TestCaseSource(nameof(NormalBuildingStates))]
        public void SetCurrentState_WhenMovingFromAlarmToNotPrevious_ReturnsFalse(string state)
        {
            BuildingController controller = new("");
            bool stateHasChanged = false;

            controller.SetCurrentState(state);
            controller.SetCurrentState(BuildingState.fireAlarm);

            foreach (string otherState in ValidBuildingStates)
            {
                if (otherState != state && otherState != BuildingState.fireAlarm)
                {
                    if (controller.SetCurrentState(otherState))
                    {
                        stateHasChanged = true;
                    }
                }
            }

            string result = controller.GetCurrentState();

            Assert.That(result, Is.EqualTo(BuildingState.fireAlarm));
            Assert.That(stateHasChanged, Is.False);
        }

        /// <summary>
        /// Test <see cref="BuildingController.SetCurrentState"/>r when transitioning from a 'fire drill' state to one different from the previous.
        /// Satisfies <strong>L2R1</strong>.
        /// </summary>
        [TestCaseSource(nameof(NormalBuildingStates))]
        public void SetCurrentState_WhenMovingFromDrillToNotPrevious_ReturnsFalse(string state)
        {
            BuildingController controller = new("");
            bool stateHasChanged = false;

            controller.SetCurrentState(state);
            controller.SetCurrentState(BuildingState.fireDrill);

            foreach (string otherState in ValidBuildingStates)
            {
                if (otherState != state && otherState != BuildingState.fireDrill)
                {
                    if (controller.SetCurrentState(otherState))
                    {
                        stateHasChanged = true;
                    }
                }
            }

            string result = controller.GetCurrentState();

            Assert.That(result, Is.EqualTo(BuildingState.fireDrill));
            Assert.That(stateHasChanged, Is.False);
        }

        // L2R2 (SetCurrentState when same state)

        /// <summary>
        /// Test <see cref="BuildingController.SetCurrentState"/> when the state is the same.
        /// Satisfies <strong>L2R2</strong>.
        /// </summary>
        [TestCaseSource(nameof(ValidBuildingStates))]
        public void SetCurrentState_WhenSameState_ReturnsTrue(string state)
        {
            BuildingController controller = new("");

            controller.SetCurrentState(state);
            bool result = controller.SetCurrentState(state);

            Assert.That(result, Is.True);
        }

        /// <summary>
        /// Test <see cref="BuildingController.SetCurrentState"/> when the state is the same.
        /// Satisfies <strong>L2R2</strong>.
        /// </summary>
        [TestCaseSource(nameof(ValidBuildingStates))]
        public void SetCurrentState_WhenSameState_RetainsState(string state)
        {
            BuildingController controller = new("");

            controller.SetCurrentState(state);
            controller.SetCurrentState(state);
            string result = controller.GetCurrentState();

            Assert.That(result, Is.EqualTo(state));
        }

        // L2R3 (Two-parameter Constructor)

        /// <summary>
        /// Test that a two-parameter constructor for <see cref="BuildingController"/> exists.
        /// Satisfies <strong>L2R3</strong>.
        /// </summary>
        [Test]
        public void Constructor_WhenTwoParameters_HasCorrectSignature()
        {
            string? firstArgName = null;
            string? secondArgName = null;
            ConstructorInfo? constructorInfoObj;
            Type[] argTypes = new Type[] { typeof(string), typeof(string) };

            // Lookup two parameter constructor
            constructorInfoObj = typeof(BuildingController).GetConstructor(argTypes);
            Assume.That(constructorInfoObj, Is.Not.Null);

            if (constructorInfoObj != null)
            {
                ParameterInfo[] constructorParams = constructorInfoObj.GetParameters();
                ParameterInfo firstParam = constructorParams.ElementAt(0);
                ParameterInfo secondParam = constructorParams.ElementAt(1);

                // Verify parameter names
                firstArgName = firstParam.Name;
                secondArgName = secondParam.Name;                
            }

            Assert.That(firstArgName, Is.EqualTo(ControllerArgNames.buildingID));
            Assert.That(secondArgName, Is.EqualTo(ControllerArgNames.startState));
        }

        /// <summary>
        /// Test constructor when using startState argument with a normal state.
        /// Satisfies <strong>L2R3</strong>.
        /// </summary>
        [Test, TestCaseSource(nameof(NormalBuildingStates))]
        public void Constructor_WhenNormalState_SetsStartState(string state)
        {
            BuildingController controller;

            controller = new BuildingController("", state);
            string result = controller.GetCurrentState();

            Assert.That(result, Is.EqualTo(state));
        }

        /// <summary>
        /// Test constructor when using startState argument with a normal state in capital letters.
        /// Satisfies <strong>L2R3</strong>.
        /// </summary>
        [TestCaseSource(nameof(NormalBuildingStates))]
        public void Constructor_WhenNormalStateCapitals_SetsStartState(string state)
        {
            BuildingController controller;

            controller = new BuildingController("", state.ToUpper());
            string result = controller.GetCurrentState();

            Assert.That(result, Is.EqualTo(state));
        }

        /// <summary>
        /// Test constructor when using startState argument with a normal state in title case.
        /// Satisfies <strong>L2R3</strong>.
        /// </summary>
        [TestCaseSource(nameof(NormalBuildingStates))]
        public void Constructor_WhenNormalStateMixedCapitals_SetsStartState(string state)
        {
            BuildingController controller;
            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;

            controller = new BuildingController("", ti.ToTitleCase(state));
            string result = controller.GetCurrentState();

            Assert.That(result, Is.EqualTo(state));
        }

        /// <summary>
        /// Test constructor when using startState argument with an invalid state.
        /// Satisfies <strong>L2R3</strong>.
        /// </summary>
        [TestCase(BuildingState.fireDrill)]
        [TestCase(BuildingState.fireAlarm)]
        [TestCaseSource(nameof(InvalidBuildingStates))]
        [TestCaseSource(nameof(TestStrings))]
        public void Constructor_WhenNotNormalState_ThrowsException(string state)
        {
            BuildingController controller;

            Assert.That(() => { controller = new("", state); },
                Throws.ArgumentException.With.Property("Message").EqualTo(ExpectedStrings.initialStateException));
        }


        // LEVEL 3 TESTS //

        /// <summary>
        /// Test that a six-parameter constructor for <see cref="BuildingController"/> exists.
        /// Satisfies <strong>L3R1</strong>.
        /// </summary>
        [Test]
        public void Constructor_WhenSixParameters_HasCorrectSignature()
        {
            ConstructorInfo? constructorInfoObj;
            bool parameterNamesMatch = true;

            Type[] argTypes = new Type[] {
                typeof(string),
                typeof(ILightManager),
                typeof(IFireAlarmManager),
                typeof(IDoorManager),
                typeof(IWebService),
                typeof(IEmailService)
            };

            string[] argNames = new string[] {
               ControllerArgNames.buildingID,
               ControllerArgNames.lightManager,
               ControllerArgNames.fireAlarmManager,
               ControllerArgNames.doorManager,
               ControllerArgNames.webService,
               ControllerArgNames.emailService
            };

            // Get constructor with 6 parameters, then check names
            constructorInfoObj = typeof(BuildingController).GetConstructor(argTypes);
            Assume.That(constructorInfoObj, Is.Not.Null);

            if (constructorInfoObj != null)
            {
                ParameterInfo[] constructorParams = constructorInfoObj.GetParameters();
                Assume.That(constructorParams, Has.Exactly(argNames.Length).Items);

                for (int i = 0; i < constructorParams.Length; i++)
                {
                    ParameterInfo parameter = constructorParams.ElementAt(i);

                    if (parameter.Name != argNames.ElementAt(i))
                    {
                        parameterNamesMatch = false;
                    }
                }
            }


            Assert.That(parameterNamesMatch, Is.True);
        }

        /// <summary>
        /// Test the <see cref="BuildingController.GetStatusReport"/> method using stubs.
        /// Satisfies <strong>L3R3</strong>.
        /// </summary>
        [Test]
        public void GetStatusReport_WhenCalled_ReturnsStatusMessages(
            // TestStrings only used in one parameter because
            // otherwise test cases would increase exponentially
            [ValueSource(nameof(LightManagerStatuses))] string lightStatus,
            [ValueSource(nameof(DoorManagerStatuses))] string doorStatus,
            [ValueSource(nameof(AlarmManagerStatuses))] [ValueSource(nameof(TestStrings))] string alarmStatus)
        {
            ILightManager lightManager = Substitute.For<ILightManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            lightManager.GetStatus().Returns(lightStatus);
            doorManager.GetStatus().Returns(doorStatus);
            fireAlarmManager.GetStatus().Returns(alarmStatus);

            BuildingController controller = new("", lightManager, fireAlarmManager, doorManager, webService, emailService);

            string result = controller.GetStatusReport();

            Assert.That(result, Is.EqualTo(string.Format("{0}{1}{2}", lightStatus, alarmStatus, doorStatus)));
        }

        /// <summary>
        /// Test the <see cref="BuildingController.SetCurrentState"/> when moving to <c>open</c> state.
        /// Satisfies <strong>L3R4</strong>.
        /// </summary>
        [Test]
        public void SetCurrentState_WhenMovingToOpenFromInitial_CallsOpenAllDoors([Values] bool doorsOpen)
        {
            ILightManager lightManager = Substitute.For<ILightManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            doorManager.OpenAllDoors().Returns(doorsOpen);

            BuildingController controller = new("", lightManager, fireAlarmManager, doorManager, webService, emailService);

            controller.SetCurrentState(BuildingState.open);

            doorManager.Received(1).OpenAllDoors();
        }

        /// <summary>
        /// Test the <see cref="BuildingController.SetCurrentState"/> when moving to <c>open</c> state.
        /// Satisfies <strong>L3R4</strong>.
        /// </summary>
        [Test]
        public void SetCurrentState_WhenMovingToOpenFromEmergency_CallsOpenAllDoors([Values(BuildingState.fireAlarm, BuildingState.fireDrill)] string initialState, [Values] bool doorsOpen)
        {
            ILightManager lightManager = Substitute.For<ILightManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            doorManager.OpenAllDoors().Returns(true);

            BuildingController controller = new("", lightManager, fireAlarmManager, doorManager, webService, emailService);
            
            // Must be in open state before switching to emergency
            controller.SetCurrentState(BuildingState.open);
            doorManager.OpenAllDoors().Returns(true);
            controller.SetCurrentState(initialState);
            doorManager.OpenAllDoors().Returns(doorsOpen);
            // Store only the last state transition's calls
            doorManager.ClearReceivedCalls();
            controller.SetCurrentState(BuildingState.open);

            doorManager.Received(1).OpenAllDoors();
        }

        /// <summary>
        /// Test the <see cref="BuildingController.SetCurrentState"/> when moving to <c>open</c> state.
        /// Satisfies <strong>L3R4</strong>, <strong>L3R5</strong>.
        /// </summary>
        [Test]
        public void SetCurrentState_WhenMovingToOpen_ReturnsBoolean([Values] bool doorsOpen)
        {
            ILightManager lightManager = Substitute.For<ILightManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            doorManager.OpenAllDoors().Returns(doorsOpen);

            BuildingController controller = new("", lightManager, fireAlarmManager, doorManager, webService, emailService);

            bool result = controller.SetCurrentState(BuildingState.open);

            Assert.That(result, Is.EqualTo(doorsOpen));
        }

        /// <summary>
        /// Test the <see cref="BuildingController.SetCurrentState"/> when moving to <c>open</c> state.
        /// Satisfies <strong>L3R4</strong>, <strong>L3R5</strong>.
        /// </summary>
        [Test]
        public void SetCurrentState_WhenMovingToOpenFromEmergency_ReturnsBoolean([Values(BuildingState.fireAlarm, BuildingState.fireDrill)] string initialState, [Values] bool doorsOpen)
        {
            ILightManager lightManager = Substitute.For<ILightManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            doorManager.OpenAllDoors().Returns(true);

            BuildingController controller = new("", lightManager, fireAlarmManager, doorManager, webService, emailService);

            controller.SetCurrentState(BuildingState.open);
            doorManager.OpenAllDoors().Returns(true);
            controller.SetCurrentState(initialState);
            doorManager.OpenAllDoors().Returns(doorsOpen);
            doorManager.ClearReceivedCalls();
            bool result = controller.SetCurrentState(BuildingState.open);

            Assert.That(result, Is.EqualTo(doorsOpen));
        }

        /// <summary>
        /// Test the <see cref="BuildingController.SetCurrentState"/> when moving to <c>open</c> state.
        /// Satisfies <strong>L3R5</strong>.
        /// </summary>
        [Test]
        public void SetCurrentState_WhenMovingToOpen_SetsState()
        {
            ILightManager lightManager = Substitute.For<ILightManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            doorManager.OpenAllDoors().Returns(true);

            BuildingController controller = new("", lightManager, fireAlarmManager, doorManager, webService, emailService);

            controller.SetCurrentState(BuildingState.open);
            string result = controller.GetCurrentState();

            Assert.That(result, Is.EqualTo(BuildingState.open));
        }

        /// <summary>
        /// Test the <see cref="BuildingController.SetCurrentState"/> when moving to <c>open</c> state.
        /// Satisfies <strong>L3R4</strong>.
        /// </summary>
        [Test]
        public void SetCurrentState_WhenMovingToOpen_DoesNotSetState()
        {
            ILightManager lightManager = Substitute.For<ILightManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            doorManager.OpenAllDoors().Returns(false);

            BuildingController controller = new("", lightManager, fireAlarmManager, doorManager, webService, emailService);

            controller.SetCurrentState(BuildingState.open);
            string result = controller.GetCurrentState();

            Assert.That(result, Is.EqualTo(BuildingState.outOfHours));
        }

        /// <summary>
        /// Test the <see cref="BuildingController.SetCurrentState"/> when moving to <c>open</c> state.
        /// Satisfies <strong>L3R5</strong>.
        /// </summary>
        [Test]
        public void SetCurrentState_WhenMovingToOpenFromEmergency_SetsState([Values(BuildingState.fireAlarm, BuildingState.fireDrill)] string initialState)
        {
            ILightManager lightManager = Substitute.For<ILightManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            doorManager.OpenAllDoors().Returns(true);

            BuildingController controller = new("", lightManager, fireAlarmManager, doorManager, webService, emailService);

            controller.SetCurrentState(BuildingState.open);
            controller.SetCurrentState(initialState);
            controller.SetCurrentState(BuildingState.open);
            string result = controller.GetCurrentState();

            Assert.That(result, Is.EqualTo(BuildingState.open));
        }

        /// <summary>
        /// Test the <see cref="BuildingController.SetCurrentState"/> when moving to <c>open</c> state.
        /// Satisfies <strong>L3R4</strong>.
        /// </summary>
        [Test]
        public void SetCurrentState_WhenMovingToOpenFromEmergency_DoesNotSetState([Values(BuildingState.fireAlarm, BuildingState.fireDrill)] string initialState)
        {
            ILightManager lightManager = Substitute.For<ILightManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            doorManager.OpenAllDoors().Returns(true);

            BuildingController controller = new("", lightManager, fireAlarmManager, doorManager, webService, emailService);

            controller.SetCurrentState(BuildingState.open);
            doorManager.OpenAllDoors().Returns(true);
            controller.SetCurrentState(initialState);
            doorManager.OpenAllDoors().Returns(false);

            controller.SetCurrentState(BuildingState.open);
            string result = controller.GetCurrentState();

            Assert.That(result, Is.EqualTo(initialState));
        }

        /// <summary>
        /// Test the <see cref="BuildingController.SetCurrentState"/>
        /// when moving to <c>open</c> state if already there.
        /// Satisfies <strong>L3R4</strong>.
        /// </summary>
        [Test]
        public void SetCurrentState_WhenMovingFromOpenToOpen_DoesNotCallOpenAllDoors()
        {
            ILightManager lightManager = Substitute.For<ILightManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            doorManager.OpenAllDoors().Returns(true);

            BuildingController controller = new("", lightManager, fireAlarmManager, doorManager, webService, emailService);

            // Since the state will be open already
            // there is no need to call the OpenAllDoors method again
            controller.SetCurrentState(BuildingState.open);
            doorManager.ClearReceivedCalls();
            controller.SetCurrentState(BuildingState.open);

            doorManager.DidNotReceive().OpenAllDoors();
        }

        // LEVEL 4 TESTS //

        // L4R1 (Moving to Closed state)

        /// <summary>
        /// Test that <see cref="BuildingController.SetCurrentState"/>
        /// calls the <see cref="IDoorManager.LockAllDoors"/> method
        /// when moving to <c>closed</c> state from the initial state.
        /// Satisfies <strong>L4R1</strong>.
        /// </summary>
        [Test]
        public void SetCurrentState_WhenMovingToClosedFromInitial_CallsLockAllDoors([Values] bool doorsLock)
        {
            ILightManager lightManager = Substitute.For<ILightManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            doorManager.LockAllDoors().Returns(doorsLock);

            BuildingController controller = new("", lightManager, fireAlarmManager, doorManager, webService, emailService);

            controller.SetCurrentState(BuildingState.closed);

            doorManager.Received(1).LockAllDoors();
        }

        /// <summary>
        /// Test that <see cref="BuildingController.SetCurrentState"/>
        /// calls the <see cref="IDoorManager.LockAllDoors"/> method
        /// when moving back to <c>closed</c> state from an emergency state.
        /// Satisfies <strong>L4R1</strong>.
        /// </summary>
        [Test]
        public void SetCurrentState_WhenMovingToClosedFromEmergency_CallsLockAllDoors([Values(BuildingState.fireAlarm, BuildingState.fireDrill)] string sourceState, [Values] bool doorsLock)
        {
            ILightManager lightManager = Substitute.For<ILightManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            doorManager.LockAllDoors().Returns(true);

            BuildingController controller = new("", lightManager, fireAlarmManager, doorManager, webService, emailService);

            controller.SetCurrentState(BuildingState.closed);
            doorManager.ClearReceivedCalls();
            doorManager.LockAllDoors().Returns(doorsLock);
            controller.SetCurrentState(sourceState);
            controller.SetCurrentState(BuildingState.closed);

            doorManager.Received(1).LockAllDoors();
        }

        /// <summary>
        /// Test that <see cref="BuildingController.SetCurrentState"/>
        /// calls the <see cref="ILightManager.SetAllLights"/> method
        /// when moving to <c>closed</c> state from the initial state.
        /// Satisfies <strong>L4R1</strong>.
        /// </summary>
        [Test]
        public void SetCurrentState_WhenMovingToClosedFromInitial_CallsSetAllLights()
        {
            ILightManager lightManager = Substitute.For<ILightManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            doorManager.LockAllDoors().Returns(true);

            BuildingController controller = new("", lightManager, fireAlarmManager, doorManager, webService, emailService);

            controller.SetCurrentState(BuildingState.closed);

            lightManager.Received(1).SetAllLights(false);
        }

        /// <summary>
        /// Test that <see cref="BuildingController.SetCurrentState"/>
        /// calls the <see cref="ILightManager.SetAllLights"/> method
        /// when moving back to <c>closed</c> state from an emergency state.
        /// Satisfies <strong>L4R1</strong>.
        /// </summary>
        [Test]
        public void SetCurrentState_WhenMovingToClosedFromEmergency_CallsSetAllLights([Values(BuildingState.fireAlarm, BuildingState.fireDrill)] string sourceState)
        {
            ILightManager lightManager = Substitute.For<ILightManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            doorManager.LockAllDoors().Returns(true);

            BuildingController controller = new("", lightManager, fireAlarmManager, doorManager, webService, emailService);

            controller.SetCurrentState(BuildingState.closed);
            controller.SetCurrentState(sourceState);
            lightManager.ClearReceivedCalls();
            controller.SetCurrentState(BuildingState.closed);

            lightManager.Received(1).SetAllLights(false);
        }

        // L4R2 (Moving to Fire Alarm state)

        /// <summary>
        /// Test that <see cref="BuildingController.SetCurrentState"/>
        /// calls the <see cref="IFireAlarmManager.SetAlarm"/> method
        /// when moving to <c>fire alarm</c> state.
        /// Satisfies <strong>L4R2</strong>.
        /// </summary>
        [TestCaseSource(nameof(NormalBuildingStates))]
        public void SetCurrentState_WhenMovingToAlarmState_CallsSetAlarm(string sourceState)
        {
            ILightManager lightManager = Substitute.For<ILightManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            doorManager.OpenAllDoors().Returns(true);
            doorManager.LockAllDoors().Returns(true);

            BuildingController controller = new BuildingController("", lightManager, fireAlarmManager, doorManager, webService, emailService);

            controller.SetCurrentState(sourceState);
            controller.SetCurrentState(BuildingState.fireAlarm);

            fireAlarmManager.Received(1).SetAlarm(true);
        }

        /// <summary>
        /// Test that <see cref="BuildingController.SetCurrentState"/>
        /// calls the <see cref="IDoorManager.OpenAllDoors"/> method
        /// when moving to <c>fire alarm</c> state.
        /// Satisfies <strong>L4R2</strong>.
        /// </summary>
        [TestCaseSource(nameof(NormalBuildingStates))]
        public void SetCurrentState_WhenMovingToAlarmState_CallsOpenAllDoors(string sourceState)
        {
            ILightManager lightManager = Substitute.For<ILightManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            doorManager.OpenAllDoors().Returns(true);
            doorManager.LockAllDoors().Returns(true);

            BuildingController controller = new("", lightManager, fireAlarmManager, doorManager, webService, emailService);

            controller.SetCurrentState(sourceState);
            doorManager.ClearReceivedCalls();
            controller.SetCurrentState(BuildingState.fireAlarm);

            doorManager.Received(1).OpenAllDoors();
        }

        /// <summary>
        /// Test that <see cref="BuildingController.SetCurrentState"/>
        /// calls the <see cref="ILightManager.SetAllLights"/> method
        /// when moving to <c>fire alarm</c> state.
        /// Satisfies <strong>L4R2</strong>.
        /// </summary>
        [TestCaseSource(nameof(NormalBuildingStates))]
        public void SetCurrentState_WhenMovingToAlarmState_CallsSetAllLights(string sourceState)
        {
            ILightManager lightManager = Substitute.For<ILightManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            doorManager.OpenAllDoors().Returns(true);
            doorManager.LockAllDoors().Returns(true);

            BuildingController controller = new("", lightManager, fireAlarmManager, doorManager, webService, emailService);

            controller.SetCurrentState(sourceState);
            controller.SetCurrentState(BuildingState.fireAlarm);

            lightManager.Received(1).SetAllLights(true);
        }

        /// <summary>
        /// Test that <see cref="BuildingController.SetCurrentState"/>
        /// calls the <see cref="IWebService.LogFireAlarm"/> method
        /// when moving to <c>fire alarm</c> state.
        /// Satisfies <strong>L4R2</strong>.
        /// </summary>
        [TestCaseSource(nameof(NormalBuildingStates))]
        public void SetCurrentState_WhenMovingToAlarmState_CallsLogFireAlarm(string sourceState)
        {
            ILightManager lightManager = Substitute.For<ILightManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            doorManager.OpenAllDoors().Returns(true);
            doorManager.LockAllDoors().Returns(true);

            BuildingController controller = new("", lightManager, fireAlarmManager, doorManager, webService, emailService);

            controller.SetCurrentState(sourceState);
            controller.SetCurrentState(BuildingState.fireAlarm);

            webService.Received(1).LogFireAlarm(BuildingState.fireAlarm);
        }

        // L4R3

        /// <summary>
        /// Test that <see cref="BuildingController.GetStatusReport"/>
        /// calls the <see cref="IWebService.LogEngineerRequired"/>
        /// method if a fault was detected.
        /// Satisfies <strong>L4R3</strong>.
        /// </summary>
        [TestCase(ManagerStatus.threeDevicesMixed, ManagerStatus.threeDevicesMixed, ManagerStatus.threeDevicesMixed)]
        [TestCase(ManagerStatus.manyDevicesOk, ManagerStatus.manyDevicesOk, ManagerStatus.singleDeviceFault)]
        [TestCase(ManagerStatus.singleDeviceFault, ManagerStatus.manyDevicesOk, ManagerStatus.singleDeviceOk)]
        [TestCase(ManagerStatus.tenDevicesFault, ManagerStatus.tenDevicesFault, ManagerStatus.tenDevicesFault)]
        [TestCase(ManagerStatus.singleDeviceFault, ManagerStatus.singleDeviceFault, ManagerStatus.singleDeviceFault)]
        public void GetStatusReport_WhenFindsFaults_CallsLogEngineerRequired(
            string lightStatus, string doorStatus, string alarmStatus)
        {
            ILightManager lightManager = Substitute.For<ILightManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            lightManager.GetStatus().Returns(ManagerStatus.lightsPrefix + lightStatus);
            doorManager.GetStatus().Returns(ManagerStatus.doorsPrefix + doorStatus);
            fireAlarmManager.GetStatus().Returns(ManagerStatus.alarmPrefix + alarmStatus);

            BuildingController controller = new("", lightManager, fireAlarmManager, doorManager, webService, emailService);

            controller.GetStatusReport();

            // Test part one of the requirement
            webService.Received(1).LogEngineerRequired(Arg.Any<string>());
        }

        /// <summary>
        /// Test that <see cref="BuildingController.GetStatusReport"/>
        /// does not call the <see cref="IWebService.LogEngineerRequired"/>
        /// method if a fault was not detected.
        /// Satisfies <strong>L4R3</strong>.
        /// </summary>
        public void GetStatusReport_WhenAllOk_DoesNotCallLogEngineerRequired(
            [ValueSource(nameof(OkManagerStatuses))] string lightStatus,
            [ValueSource(nameof(OkManagerStatuses))] string doorStatus,
            [ValueSource(nameof(OkManagerStatuses))] string alarmStatus)
        {
            ILightManager lightManager = Substitute.For<ILightManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            lightManager.GetStatus().Returns(ManagerStatus.lightsPrefix + lightStatus);
            doorManager.GetStatus().Returns(ManagerStatus.doorsPrefix + doorStatus);
            fireAlarmManager.GetStatus().Returns(ManagerStatus.alarmPrefix + alarmStatus);

            BuildingController controller = new("", lightManager, fireAlarmManager, doorManager, webService, emailService);

            controller.GetStatusReport();

            webService.DidNotReceive().LogEngineerRequired(Arg.Any<string>());
        }

        /// <summary>
        /// Test that <see cref="BuildingController.GetStatusReport"/>
        /// calls the <see cref="IWebService.LogEngineerRequired"/>
        /// method if a fault was detected in the lights manager.
        /// Satisfies <strong>L4R3</strong>.
        /// </summary>
        [Test]
        public void GetStatusReport_WhenFindsFaultsSingleManagerInLights_CallsLogEngineerRequired(
            [ValueSource(nameof(FaultyManagerStatuses))] string lightStatus,
            [ValueSource(nameof(OkManagerStatuses))] string doorStatus,
            [ValueSource(nameof(OkManagerStatuses))] string alarmStatus)
        {
            ILightManager lightManager = Substitute.For<ILightManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            lightManager.GetStatus().Returns(ManagerStatus.lightsPrefix + lightStatus);
            doorManager.GetStatus().Returns(ManagerStatus.doorsPrefix + doorStatus);
            fireAlarmManager.GetStatus().Returns(ManagerStatus.alarmPrefix + alarmStatus);

            BuildingController controller = new("", lightManager, fireAlarmManager, doorManager, webService, emailService);

            controller.GetStatusReport();

            webService.Received(1).LogEngineerRequired(ManagerStatus.lights);
        }

        /// <summary>
        /// Test that <see cref="BuildingController.GetStatusReport"/>
        /// calls the <see cref="IWebService.LogEngineerRequired"/>
        /// method if a fault was detected in the doors manager.
        /// Satisfies <strong>L4R3</strong>.
        /// </summary>
        [Test]
        public void GetStatusReport_WhenFindsFaultsSingleManagerInDoors_CallsLogEngineerRequired(
            [ValueSource(nameof(OkManagerStatuses))] string lightStatus,
            [ValueSource(nameof(FaultyManagerStatuses))] string doorStatus,
            [ValueSource(nameof(OkManagerStatuses))] string alarmStatus)
        {
            ILightManager lightManager = Substitute.For<ILightManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            lightManager.GetStatus().Returns(ManagerStatus.lightsPrefix + lightStatus);
            doorManager.GetStatus().Returns(ManagerStatus.doorsPrefix + doorStatus);
            fireAlarmManager.GetStatus().Returns(ManagerStatus.alarmPrefix + alarmStatus);

            BuildingController controller = new("", lightManager, fireAlarmManager, doorManager, webService, emailService);

            controller.GetStatusReport();

            webService.Received(1).LogEngineerRequired(ManagerStatus.doors);
        }

        /// <summary>
        /// Test that <see cref="BuildingController.GetStatusReport"/>
        /// calls the <see cref="IWebService.LogEngineerRequired"/>
        /// method if a fault was detected in the fire alarm manager.
        /// Satisfies <strong>L4R3</strong>.
        /// </summary>
        [Test]
        public void GetStatusReport_WhenFindsFaultsSingleManagerInAlarm_CallsLogEngineerRequired(
            [ValueSource(nameof(OkManagerStatuses))] string lightStatus,
            [ValueSource(nameof(OkManagerStatuses))] string doorStatus,
            [ValueSource(nameof(FaultyManagerStatuses))] string alarmStatus)
        {
            ILightManager lightManager = Substitute.For<ILightManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            lightManager.GetStatus().Returns(ManagerStatus.lightsPrefix + lightStatus);
            doorManager.GetStatus().Returns(ManagerStatus.doorsPrefix + doorStatus);
            fireAlarmManager.GetStatus().Returns(ManagerStatus.alarmPrefix + alarmStatus);

            BuildingController controller = new("", lightManager, fireAlarmManager, doorManager, webService, emailService);

            controller.GetStatusReport();

            webService.Received(1).LogEngineerRequired(ManagerStatus.alarm);
        }

        /// <summary>
        /// Test the <see cref="BuildingController.GetStatusReport"/> method using stubs.
        /// Satisfies <strong>L4R3</strong>.
        /// </summary>
        [Test]
        public void GetStatusReport_WhenFindsFaultsAllManagers_CallsLogEngineerRequired(
            [ValueSource(nameof(FaultyManagerStatuses))] string lightStatus,
            [ValueSource(nameof(FaultyManagerStatuses))] string doorStatus,
            [ValueSource(nameof(FaultyManagerStatuses))] string alarmStatus)
        {
            ILightManager lightManager = Substitute.For<ILightManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            lightManager.GetStatus().Returns(ManagerStatus.lightsPrefix + lightStatus);
            doorManager.GetStatus().Returns(ManagerStatus.doorsPrefix + doorStatus);
            fireAlarmManager.GetStatus().Returns(ManagerStatus.alarmPrefix + alarmStatus);

            BuildingController controller = new("", lightManager, fireAlarmManager, doorManager, webService, emailService);

            controller.GetStatusReport();

            webService.Received(1).LogEngineerRequired(string.Format("{0}{1}{2}",
                ManagerStatus.lightsPrefix, ManagerStatus.alarmPrefix, ManagerStatus.doorsPrefix));
        }

        /// <summary>
        /// Test the <see cref="BuildingController.GetStatusReport"/> method using stubs.
        /// Satisfies <strong>L4R3</strong>.
        /// </summary>
        [Test]
        public void GetStatusReport_WhenFindsFaultsLightsAndDoors_CallsLogEngineerRequired(
            [ValueSource(nameof(FaultyManagerStatuses))] string lightStatus,
            [ValueSource(nameof(FaultyManagerStatuses))] string doorStatus,
            [ValueSource(nameof(OkManagerStatuses))] string alarmStatus)
        {
            ILightManager lightManager = Substitute.For<ILightManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            lightManager.GetStatus().Returns(ManagerStatus.lightsPrefix + lightStatus);
            doorManager.GetStatus().Returns(ManagerStatus.doorsPrefix + doorStatus);
            fireAlarmManager.GetStatus().Returns(ManagerStatus.alarmPrefix + alarmStatus);

            BuildingController controller = new("", lightManager, fireAlarmManager, doorManager, webService, emailService);

            controller.GetStatusReport();

            webService.Received(1).LogEngineerRequired(string.Format("{0}{1}",
                ManagerStatus.lightsPrefix, ManagerStatus.doorsPrefix));
        }

        /// <summary>
        /// Test the <see cref="BuildingController.GetStatusReport"/> method using stubs.
        /// Satisfies <strong>L4R3</strong>.
        /// </summary>
        [Test]
        public void GetStatusReport_WhenFindsFaultsLightsAndAlarm_CallsLogEngineerRequired(
            [ValueSource(nameof(FaultyManagerStatuses))] string lightStatus,
            [ValueSource(nameof(OkManagerStatuses))] string doorStatus,
            [ValueSource(nameof(FaultyManagerStatuses))] string alarmStatus)
        {
            ILightManager lightManager = Substitute.For<ILightManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            lightManager.GetStatus().Returns(ManagerStatus.lightsPrefix + lightStatus);
            doorManager.GetStatus().Returns(ManagerStatus.doorsPrefix + doorStatus);
            fireAlarmManager.GetStatus().Returns(ManagerStatus.alarmPrefix + alarmStatus);

            BuildingController controller = new("", lightManager, fireAlarmManager, doorManager, webService, emailService);

            controller.GetStatusReport();

            webService.Received(1).LogEngineerRequired(string.Format("{0}{1}",
                ManagerStatus.lightsPrefix, ManagerStatus.alarmPrefix));
        }

        /// <summary>
        /// Test the <see cref="BuildingController.GetStatusReport"/> method using stubs.
        /// Satisfies <strong>L4R3</strong>.
        /// </summary>
        [Test]
        public void GetStatusReport_WhenFindsFaultsDoorsAndAlarm_CallsLogEngineerRequired(
            [ValueSource(nameof(OkManagerStatuses))] string lightStatus,
            [ValueSource(nameof(FaultyManagerStatuses))] string doorStatus,
            [ValueSource(nameof(FaultyManagerStatuses))] string alarmStatus)
        {
            ILightManager lightManager = Substitute.For<ILightManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            lightManager.GetStatus().Returns(ManagerStatus.lightsPrefix + lightStatus);
            doorManager.GetStatus().Returns(ManagerStatus.doorsPrefix + doorStatus);
            fireAlarmManager.GetStatus().Returns(ManagerStatus.alarmPrefix + alarmStatus);

            BuildingController controller = new("", lightManager, fireAlarmManager, doorManager, webService, emailService);

            controller.GetStatusReport();

            webService.Received(1).LogEngineerRequired(string.Format("{0}{1}",
                ManagerStatus.alarmPrefix, ManagerStatus.doorsPrefix));
        }

        // L4R4 

        /// <summary>
        /// Test that <see cref="BuildingController.SetCurrentState"/>
        /// calls the <see cref="IEmailService.SendEmail"/> method
        /// when moving to <c>fire alarm</c> state.
        /// Satisfies <strong>L4R4</strong>.
        /// </summary>
        public void SetCurrentState_WhenMovingToAlarmState_CallsSendEmail(
            [ValueSource(nameof(NormalBuildingStates))] string sourceState,
            [ValueSource(nameof(TestStrings))] string errorMessage)
        {
            ILightManager lightManager = Substitute.For<ILightManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            doorManager.OpenAllDoors().Returns(true);
            doorManager.LockAllDoors().Returns(true);

            // Set mock to throw exception if method is called
            webService.WhenForAnyArgs(x => x.LogFireAlarm(BuildingState.fireAlarm)).Do(x => { throw new Exception(errorMessage); });

            BuildingController controller = new("", lightManager, fireAlarmManager, doorManager, webService, emailService);         

            controller.SetCurrentState(sourceState);
            controller.SetCurrentState(BuildingState.fireAlarm);

            // Assert method call with exception message
            emailService.Received(1).SendEmail(
                ExpectedStrings.emailAddress,
                ExpectedStrings.emailSubject,
                Arg.Is<string>(x => x.Contains(errorMessage))
            );
        }
    }
}