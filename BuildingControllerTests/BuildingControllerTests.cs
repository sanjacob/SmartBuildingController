using NUnit.Framework;
using System.Reflection;
using SmartBuilding;
using System.Globalization;
using NSubstitute;

namespace BuildingControllerTests
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
    /// Allowed exception messages for <see cref="BuildingController"/>.
    /// </summary>
    struct ControllerExceptions
    {
        public const string initialStateException = "Argument Exception: BuildingController can only be initialised "
            + "to the following states 'open', 'closed', 'out of hours'";
    }

    [TestFixture]
    [Author("Jacob Sanchez", "jsanchez-perez@uclan.ac.uk")]
    public class BuildingControllerTests
    {
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

        private static readonly object?[] LightManagerStatuses =
        {
            "Lights,",
            "Lights,OK,FAULT,OK,",
            "Lights,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,",
            "Lights,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,",
            "Lights,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK," +
                "OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK," +
                "OK,OK,OK,OK,OK,OK,OK,",
        };

        private static readonly object?[] DoorManagerStatuses =
        {
            "Doors,",
            "Doors,OK,FAULT,OK,",
            "Doors,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,",
            "Doors,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,",
            "Doors,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK," +
                "OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK," +
                "OK,OK,OK,OK,OK,OK,OK,",
        };

        private static readonly object?[] AlarmManagerStatuses =
        {
            "FireAlarm,",
            "FireAlarm,OK,FAULT,OK,",
            "FireAlarm,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,",
            "FireAlarm,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,",
            "FireAlarm,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK," +
                "OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK," +
                "OK,OK,OK,OK,OK,OK,OK,",
        };


        // LEVEL 1 TESTS //

        /// <summary>
        /// Test that a valid constructor exists for <class>BuildingController</class> through reflection.
        /// Satisfies <req>L1R1</req>.
        /// </summary>
        [Test]
        public void Constructor_WhenSingleParameter_HasCorrectSignature()
        {
            ConstructorInfo? constructorInfoObj;
            Type[] argTypes = new Type[] { typeof(string) };

            constructorInfoObj = typeof(BuildingController).GetConstructor(argTypes);

            Assume.That(constructorInfoObj, Is.Not.Null);

            if (constructorInfoObj != null)
            {
                ParameterInfo[] constructorParams = constructorInfoObj.GetParameters();
                ParameterInfo? firstParam = constructorParams.First();

                Assume.That(firstParam, Is.Not.Null);

                if (firstParam != null)
                {
                    string argName = firstParam.Name == null ? "" : firstParam.Name;
                    Assert.That(argName, Is.EqualTo(ControllerArgNames.buildingID));
                }
            }
        }

        /// <summary>
        /// Test initialisation of <c>buildingID</c> when constructor parameter set.
        /// Satisfies <req>L1R2</req>, <req>L1R3</req>.
        /// </summary>
        [TestCase("Building ID")]
        [TestCaseSource(nameof(TestStrings))]
        public void Constructor_WhenSet_InitialisesBuildingID(string buildingID)
        {
            BuildingController controller;

            controller = new BuildingController(buildingID);
            string result = controller.GetBuildingID();

            Assert.That(result, Is.EqualTo(buildingID.ToLower()));
        }

        /// <summary>
        /// Test <c>buildingID</c> setter.
        /// Satisfies <req>L1R4</req>.
        /// </summary>
        [TestCase("Building ID")]
        [TestCaseSource(nameof(TestStrings))]
        public void SetBuildingID_WhenSet_SetsID(string buildingID)
        {
            BuildingController controller;

            controller = new BuildingController("");
            controller.SetBuildingID(buildingID);
            string result = controller.GetBuildingID();

            Assert.That(result, Is.EqualTo(buildingID.ToLower()));
        }

        /// <summary>
        /// Test initialisation of <c>currentState</c>.
        /// Satisfies <req>L1R5</req>, <req>L1R6</req>.
        /// </summary>
        [Test]
        public void Constructor_ByDefault_InitialisesCurrentState()
        {
            BuildingController controller;

            controller = new BuildingController("");
            string result = controller.GetCurrentState();

            Assert.That(result, Is.EqualTo(BuildingState.outOfHours));
        }

        // L1R7 (SetCurrentState from default startup)

        /// <summary>
        /// Test <c>currentState</c> setter with valid states.
        /// Satisfies <req>L1R7</req>.
        /// </summary>
        [Test, TestCaseSource(nameof(ValidBuildingStates))]
        public void SetCurrentState_WhenValidState_ReturnsTrue(string state)
        {
            BuildingController controller;

            controller = new BuildingController("");
            bool result = controller.SetCurrentState(state);

            Assert.That(result, Is.True);
        }

        /// <summary>
        /// Test <c>currentState</c> setter with valid states.
        /// Satisfies <req>L1R7</req>.
        /// </summary>
        [TestCaseSource(nameof(ValidBuildingStates))]
        public void SetCurrentState_WhenValidState_SetsState(string state)
        {
            BuildingController controller;

            controller = new BuildingController("");
            controller.SetCurrentState(state);
            string result = controller.GetCurrentState();

            Assert.That(result, Is.EqualTo(state));
        }

        /// <summary>
        /// Test <c>currentState</c> setter with invalid states.
        /// Satisfies <req>L1R7</req>.
        /// </summary>
        [Test]
        public void SetCurrentState_WhenInvalidState_ReturnsFalse(
            [ValueSource(nameof(InvalidBuildingStates))] [ValueSource(nameof(TestStrings))] string state,
            [ValueSource(nameof(ValidBuildingStates))] string sourceState)
        {
            BuildingController controller;

            controller = new BuildingController("");
            controller.SetCurrentState(sourceState);
            bool result = controller.SetCurrentState(state);

            Assert.That(result, Is.False);
        }

        /// <summary>
        /// Test <c>currentState</c> setter with invalid states.
        /// Satisfies <req>L1R7</req>.
        /// </summary>
        [Test]
        public void SetCurrentState_WhenInvalidState_DoesNotSetState(
            [ValueSource(nameof(InvalidBuildingStates))] [ValueSource(nameof(TestStrings))] string state,
            [ValueSource(nameof(ValidBuildingStates))] string sourceState)
        {
            BuildingController controller;

            controller = new BuildingController("");
            controller.SetCurrentState(sourceState);
            controller.SetCurrentState(state);
            string result = controller.GetCurrentState();

            Assert.That(result, Is.EqualTo(sourceState));
        }


        // LEVEL 2 TESTS //

        // L2R1 (STD)

        // From Normal States

        /// <summary>
        /// Test <c>currentState</c> setter when transitioning from 'closed' state.
        /// Satisfies <req>L2R1</req>.
        /// </summary>
        [TestCase(BuildingState.outOfHours)]
        [TestCase(BuildingState.fireDrill)]
        [TestCase(BuildingState.fireAlarm)]
        [TestCase(BuildingState.open, false)]
        public void SetCurrentState_WhenCurrentStateClosed_ReturnsBoolean(string state, bool success = true)
        {
            BuildingController controller;

            controller = new BuildingController("");
            controller.SetCurrentState(BuildingState.closed);
            bool result = controller.SetCurrentState(state);

            Assert.That(result, Is.EqualTo(success));
        }

        /// <summary>
        /// Test <c>currentState</c> setter when transitioning from 'open' state.
        /// Satisfies <req>L2R1</req>.
        /// </summary>
        [TestCase(BuildingState.outOfHours)]
        [TestCase(BuildingState.fireDrill)]
        [TestCase(BuildingState.fireAlarm)]
        [TestCase(BuildingState.closed, false)]
        public void SetCurrentState_WhenCurrentStateOpen_ReturnsBoolean(string state, bool success = true)
        {
            BuildingController controller;

            controller = new BuildingController("");
            controller.SetCurrentState(BuildingState.open);
            bool result = controller.SetCurrentState(state);

            Assert.That(result, Is.EqualTo(success));
        }

        /// <summary>
        /// Test <c>currentState</c> setter when transitioning from 'open' state.
        /// Satisfies <req>L2R1</req>.
        /// </summary>
        [TestCase(BuildingState.outOfHours)]
        [TestCase(BuildingState.fireDrill)]
        [TestCase(BuildingState.fireAlarm)]
        public void SetCurrentState_WhenCurrentStateOpen_SetsState(string state)
        {
            BuildingController controller;

            controller = new BuildingController("");
            controller.SetCurrentState(BuildingState.open);
            controller.SetCurrentState(state);
            string result = controller.GetCurrentState();

            Assert.That(result, Is.EqualTo(state));
        }

        /// <summary>
        /// Test <c>currentState</c> setter when transitioning from 'closed' state.
        /// Satisfies <req>L2R1</req>.
        /// </summary>
        [TestCase(BuildingState.outOfHours)]
        [TestCase(BuildingState.fireDrill)]
        [TestCase(BuildingState.fireAlarm)]
        public void SetCurrentState_WhenCurrentStateClosed_SetsState(string state)
        {
            BuildingController controller;

            controller = new BuildingController("");
            controller.SetCurrentState(BuildingState.closed);
            controller.SetCurrentState(state);
            string result = controller.GetCurrentState();

            Assert.That(result, Is.EqualTo(state));
        }

        /// <summary>
        /// Test <c>currentState</c> setter when transitioning from 'closed' state to 'open'.
        /// Satisfies <req>L2R1</req>.
        /// </summary>
        [TestCase(BuildingState.open)]
        public void SetCurrentState_WhenCurrentStateClosed_DoesNotSetState(string state)
        {
            BuildingController controller;

            controller = new BuildingController("");
            controller.SetCurrentState(BuildingState.closed);
            controller.SetCurrentState(state);
            string result = controller.GetCurrentState();

            Assert.That(result, Is.EqualTo(BuildingState.closed));
        }

        /// <summary>
        /// Test <c>currentState</c> setter when transitioning from 'open' state to 'closed'.
        /// Satisfies <req>L2R1</req>.
        /// </summary>
        [TestCase(BuildingState.closed)]
        public void SetCurrentState_WhenCurrentStateOpen_DoesNotSetState(string state)
        {
            BuildingController controller;

            controller = new BuildingController("");
            controller.SetCurrentState(BuildingState.open);
            controller.SetCurrentState(state);
            string result = controller.GetCurrentState();

            Assert.That(result, Is.EqualTo(BuildingState.open));
        }

        // Emergency States

        /// <summary>
        /// Test <c>currentState</c> setter when transitioning from a 'fire alarm' state to the previous one.
        /// Satisfies <req>L2R1</req>.
        /// </summary>
        [TestCaseSource(nameof(NormalBuildingStates))]
        public void SetCurrentState_WhenCurrentStateAlarmAndPreviousState_ReturnsTrue(string state)
        {
            BuildingController controller;

            controller = new BuildingController("");
            controller.SetCurrentState(state);
            controller.SetCurrentState(BuildingState.fireAlarm);
            bool result = controller.SetCurrentState(state);
            string newState = controller.GetCurrentState();

            Assert.That(result, Is.True);
            Assert.That(newState, Is.EqualTo(state));
        }

        /// <summary>
        /// Test <c>currentState</c> setter when transitioning from a 'fire drill' state to the previous one.
        /// Satisfies <req>L2R1</req>.
        /// </summary>
        [TestCaseSource(nameof(NormalBuildingStates))]
        public void SetCurrentState_WhenCurrentStateDrillAndPreviousState_ReturnsTrue(string state)
        {
            BuildingController controller;

            controller = new BuildingController("");
            controller.SetCurrentState(state);
            controller.SetCurrentState(BuildingState.fireDrill);
            bool result = controller.SetCurrentState(state);
            string newState = controller.GetCurrentState();

            Assert.That(result, Is.True);
            Assert.That(newState, Is.EqualTo(state));
        }

        /// <summary>
        /// Test <c>currentState</c> setter when transitioning from a 'fire alarm' state to one different from the previous.
        /// Satisfies <req>L2R1</req>.
        /// </summary>
        [TestCaseSource(nameof(NormalBuildingStates))]
        public void SetCurrentState_WhenCurrentStateAlarmAndNotPreviousState_ReturnsFalse(string state)
        {
            BuildingController controller;

            controller = new BuildingController("");
            controller.SetCurrentState(state);
            controller.SetCurrentState(BuildingState.fireAlarm);

            bool stateHasChanged = false;

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
        /// Test <c>currentState</c> setter when transitioning from a 'fire drill' state to one different from the previous.
        /// Satisfies <req>L2R1</req>.
        /// </summary>
        [TestCaseSource(nameof(NormalBuildingStates))]
        public void SetCurrentState_WhenCurrentStateDrillAndNotPreviousState_ReturnsFalse(string state)
        {
            BuildingController controller;

            controller = new BuildingController("");
            controller.SetCurrentState(state);
            controller.SetCurrentState(BuildingState.fireDrill);

            bool stateHasChanged = false;

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
        /// Test <c>currentState</c> setter when the state is the same.
        /// Satisfies <req>L2R2</req>.
        /// </summary>
        [TestCaseSource(nameof(ValidBuildingStates))]
        public void SetCurrentState_WhenSameState_ReturnsTrue(string state)
        {
            BuildingController controller;

            controller = new BuildingController("");
            controller.SetCurrentState(state);

            bool result = controller.SetCurrentState(state);
            Assert.That(result, Is.True);
        }

        /// <summary>
        /// Test <c>currentState</c> setter when the state is the same.
        /// Satisfies <req>L2R2</req>.
        /// </summary>
        [TestCaseSource(nameof(ValidBuildingStates))]
        public void SetCurrentState_WhenSameState_RetainsState(string state)
        {
            BuildingController controller;

            controller = new BuildingController("");
            controller.SetCurrentState(state);
            controller.SetCurrentState(state);

            string result = controller.GetCurrentState();
            Assert.That(result, Is.EqualTo(state));
        }

        // L2R3 (Two-parameter Constructor)

        /// <summary>
        /// Test that a two-parameter constructor for <see cref="BuildingController"/> exists.
        /// Satisfies <req>L2R3</req>.
        /// </summary>
        [Test]
        public void Constructor_WhenTwoParameters_HasCorrectSignature()
        {
            ConstructorInfo? constructorInfoObj;
            Type[] argTypes = new Type[] { typeof(string), typeof(string) };

            constructorInfoObj = typeof(BuildingController).GetConstructor(argTypes);

            Assume.That(constructorInfoObj, Is.Not.Null);

            if (constructorInfoObj != null)
            {
                ParameterInfo[] constructorParams = constructorInfoObj.GetParameters();
                ParameterInfo? firstParam = constructorParams.ElementAt(0);
                ParameterInfo? secondParam = constructorParams.ElementAt(1);

                Assume.That(firstParam, Is.Not.Null);
                Assume.That(secondParam, Is.Not.Null);

                if (firstParam != null && secondParam != null)
                {
                    string firstArgName = firstParam.Name == null ? "" : firstParam.Name;
                    string secondArgName = secondParam.Name == null ? "" : secondParam.Name;

                    Assert.That(firstArgName, Is.EqualTo(ControllerArgNames.buildingID));
                    Assert.That(secondArgName, Is.EqualTo(ControllerArgNames.startState));
                }
            }
        }

        /// <summary>
        /// Test constructor when using startState argument with a normal state.
        /// Satisfies <req>L2R3</req>.
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
        /// Satisfies <req>L2R3</req>.
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
        /// Satisfies <req>L2R3</req>.
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
        /// Test constructor when using startState argument with an emergency / invalid state.
        /// Satisfies <req>L2R3</req>.
        /// </summary>
        [TestCase(BuildingState.fireDrill)]
        [TestCase(BuildingState.fireAlarm)]
        [TestCaseSource(nameof(InvalidBuildingStates))]
        [TestCaseSource(nameof(TestStrings))]
        public void Constructor_WhenEmergencyState_ThrowsException(string state)
        {
            Assert.That(() => { new BuildingController("", state); },
                Throws.ArgumentException.With.Property("Message").EqualTo(ControllerExceptions.initialStateException));
        }


        // LEVEL 3 TESTS //

        /// <summary>
        /// Test that a six-parameter constructor for <see cref="BuildingController"/> exists.
        /// Satisfies <req>L3R1</req>.
        /// </summary>
        [Test]
        public void Constructor_WhenSixParameters_HasCorrectSignature()
        {
            ConstructorInfo? constructorInfoObj;

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

            constructorInfoObj = typeof(BuildingController).GetConstructor(argTypes);

            Assume.That(constructorInfoObj, Is.Not.Null);

            if (constructorInfoObj != null)
            {
                ParameterInfo[] constructorParams = constructorInfoObj.GetParameters();
                Assume.That(constructorParams, Has.Exactly(argNames.Length).Items);

                bool parameterNamesMatch = true;

                for (int i = 0; i < constructorParams.Length; i++)
                {
                    ParameterInfo parameter = constructorParams.ElementAt(i);
                    string paramName = parameter.Name == null ? "" : parameter.Name;

                    if (paramName != argNames.ElementAt(i))
                    {
                        parameterNamesMatch = false;
                    }
                }

                Assert.That(parameterNamesMatch, Is.True);
            }
        }

        /// <summary>
        /// Test the <see cref="BuildingController.GetStatusReport"/> method using stubs.
        /// Satisfies <req>L3R3</req>.
        /// </summary>
        [Test]
        public void GetStatusReport_WhenCalled_ReturnsStatusMessages(
            // TestStrings only used in one parameter because
            // otherwise test cases would increase exponentially
            [ValueSource(nameof(LightManagerStatuses))] string lightStatus,
            [ValueSource(nameof(DoorManagerStatuses))] string doorStatus,
            [ValueSource(nameof(IFireAlarmManager))] [ValueSource(nameof(TestStrings))] string alarmStatus)
        {
            BuildingController controller;

            ILightManager lightManager = Substitute.For<ILightManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();

            lightManager.GetStatus().Returns(lightStatus);
            doorManager.GetStatus().Returns(doorStatus);
            fireAlarmManager.GetStatus().Returns(alarmStatus); 

            controller = new BuildingController("", lightManager, fireAlarmManager, doorManager, webService, emailService);
            string result = controller.GetStatusReport();

            Assert.That(result, Is.EqualTo(string.Format("{0}{1}{2}", lightStatus, doorStatus, alarmStatus)));
        }

        /// <summary>
        /// Test the <see cref="BuildingController.SetCurrentState(string)"/> when moving to <c>open</c> state.
        /// Satisfies <req>L3R4</req>.
        /// </summary>
        [Test]
        public void SetCurrentState_WhenMovingToOpen_CallsOpenAllDoors([Values(true, false)] bool doorsOpen)
        {
            BuildingController controller;

            ILightManager lightManager = Substitute.For<ILightManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();

            doorManager.OpenAllDoors().Returns(doorsOpen);
            controller = new BuildingController("", lightManager, fireAlarmManager, doorManager, webService, emailService);
            controller.SetCurrentState(BuildingState.open);

            bool result = doorManager.Received(1).OpenAllDoors();

            Assert.That(result, Is.True);
        }

        /// <summary>
        /// Test the <see cref="BuildingController.SetCurrentState(string)"/> when moving to <c>open</c> state.
        /// Satisfies <req>L3R4</req>.
        /// </summary>
        [Test]
        public void SetCurrentState_WhenMovingToOpen_CallsOpenAllDoors([Values(BuildingState.fireAlarm, BuildingState.fireDrill)] string initialState, [Values(true, false)] bool doorsOpen)
        {
            BuildingController controller;

            ILightManager lightManager = Substitute.For<ILightManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();

            controller = new BuildingController("", lightManager, fireAlarmManager, doorManager, webService, emailService);
            doorManager.OpenAllDoors().Returns(true);
            controller.SetCurrentState(BuildingState.open);
            doorManager.ClearReceivedCalls();

            doorManager.OpenAllDoors().Returns(doorsOpen);
            controller.SetCurrentState(initialState);
            controller.SetCurrentState(BuildingState.open);

            bool result = doorManager.Received(1).OpenAllDoors();

            Assert.That(result, Is.True);
        }

        /// <summary>
        /// Test the <see cref="BuildingController.SetCurrentState(string)"/> when moving to <c>open</c> state.
        /// Satisfies <req>L3R4</req>, <req>L3R5</req>.
        /// </summary>
        [Test]
        public void SetCurrentState_WhenMovingToOpen_ReturnsBoolean([Values(true, false)] bool doorsOpen)
        {
            BuildingController controller;

            ILightManager lightManager = Substitute.For<ILightManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();

            doorManager.OpenAllDoors().Returns(doorsOpen);
            controller = new BuildingController("", lightManager, fireAlarmManager, doorManager, webService, emailService);
            bool result = controller.SetCurrentState(BuildingState.open);

            Assert.That(result, Is.EqualTo(doorsOpen));
        }

        /// <summary>
        /// Test the <see cref="BuildingController.SetCurrentState(string)"/> when moving to <c>open</c> state.
        /// Satisfies <req>L3R4</req>, <req>L3R5</req>.
        /// </summary>
        [Test]
        public void SetCurrentState_WhenMovingToOpenFromEmergency_ReturnsBoolean([Values(BuildingState.fireAlarm, BuildingState.fireDrill)] string initialState, [Values(true, false)] bool doorsOpen)
        {
            BuildingController controller;

            ILightManager lightManager = Substitute.For<ILightManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();

            controller = new BuildingController("", lightManager, fireAlarmManager, doorManager, webService, emailService);
            doorManager.OpenAllDoors().Returns(true);
            controller.SetCurrentState(BuildingState.open);
            doorManager.ClearReceivedCalls();

            doorManager.OpenAllDoors().Returns(doorsOpen);
            controller.SetCurrentState(initialState);
            bool result = controller.SetCurrentState(BuildingState.open);

            Assert.That(result, Is.EqualTo(doorsOpen));
        }

        /// <summary>
        /// Test the <see cref="BuildingController.SetCurrentState(string)"/> when moving to <c>open</c> state.
        /// Satisfies <req>L3R5</req>.
        /// </summary>
        [Test]
        public void SetCurrentState_WhenMovingToOpen_SetsState()
        {
            BuildingController controller;

            ILightManager lightManager = Substitute.For<ILightManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();

            doorManager.OpenAllDoors().Returns(true);
            controller = new BuildingController("", lightManager, fireAlarmManager, doorManager, webService, emailService);
            controller.SetCurrentState(BuildingState.open);
            string result = controller.GetCurrentState();

            Assert.That(result, Is.EqualTo(BuildingState.open));
        }

        /// <summary>
        /// Test the <see cref="BuildingController.SetCurrentState(string)"/> when moving to <c>open</c> state.
        /// Satisfies <req>L3R4</req>.
        /// </summary>
        [Test]
        public void SetCurrentState_WhenMovingToOpen_DoesNotSetState()
        {
            BuildingController controller;

            ILightManager lightManager = Substitute.For<ILightManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();

            doorManager.OpenAllDoors().Returns(false);
            controller = new BuildingController("", lightManager, fireAlarmManager, doorManager, webService, emailService);
            controller.SetCurrentState(BuildingState.open);
            string result = controller.GetCurrentState();

            Assert.That(result, Is.EqualTo(BuildingState.outOfHours));
        }

        /// <summary>
        /// Test the <see cref="BuildingController.SetCurrentState(string)"/> when moving to <c>open</c> state.
        /// Satisfies <req>L3R5</req>.
        /// </summary>
        [Test]
        public void SetCurrentState_WhenMovingToOpenFromEmergency_SetsState([Values(BuildingState.fireAlarm, BuildingState.fireDrill)] string initialState, [Values(true, false)] bool doorsOpen)
        {
            BuildingController controller;

            ILightManager lightManager = Substitute.For<ILightManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();

            controller = new BuildingController("", lightManager, fireAlarmManager, doorManager, webService, emailService);
            doorManager.OpenAllDoors().Returns(true);
            controller.SetCurrentState(BuildingState.open);
            doorManager.ClearReceivedCalls();

            doorManager.OpenAllDoors().Returns(doorsOpen);
            controller.SetCurrentState(initialState);
            controller.SetCurrentState(BuildingState.open);
            string result = controller.GetCurrentState();

            Assert.That(result, Is.EqualTo(BuildingState.open));
        }

        /// <summary>
        /// Test the <see cref="BuildingController.SetCurrentState(string)"/> when moving to <c>open</c> state.
        /// Satisfies <req>L3R4</req>.
        /// </summary>
        [Test]
        public void SetCurrentState_WhenMovingToOpenFromEmergency_DoesNotSetState([Values(BuildingState.fireAlarm, BuildingState.fireDrill)] string initialState, [Values(true, false)] bool doorsOpen)
        {
            BuildingController controller;

            ILightManager lightManager = Substitute.For<ILightManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();

            controller = new BuildingController("", lightManager, fireAlarmManager, doorManager, webService, emailService);
            doorManager.OpenAllDoors().Returns(true);
            controller.SetCurrentState(BuildingState.open);
            doorManager.ClearReceivedCalls();

            doorManager.OpenAllDoors().Returns(doorsOpen);
            controller.SetCurrentState(initialState);
            controller.SetCurrentState(BuildingState.open);
            string result = controller.GetCurrentState();

            Assert.That(result, Is.EqualTo(initialState));
        }

        /// <summary>
        /// Test the <see cref="BuildingController.SetCurrentState(string)"/> when moving to <c>open</c> state if already there.
        /// Satisfies <req>L3R4</req>.
        /// </summary>
        [Test]
        public void SetCurrentState_WhenMovingFromOpenToOpen_DoesNotCallOpenAllDoors()
        {
            BuildingController controller;

            ILightManager lightManager = Substitute.For<ILightManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();

            doorManager.OpenAllDoors().Returns(true);
            controller = new BuildingController("", lightManager, fireAlarmManager, doorManager, webService, emailService);
            controller.SetCurrentState(BuildingState.open);
            doorManager.ClearReceivedCalls();
            controller.SetCurrentState(BuildingState.open);

            bool result = doorManager.DidNotReceive().OpenAllDoors();

            Assert.That(result, Is.True);
        }
    }
}