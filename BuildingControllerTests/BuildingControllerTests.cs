using NUnit.Framework;
using System.Reflection;
using SmartBuilding;
using System.Globalization;

namespace BuildingControllerTests
{
    /// <summary>
    /// Contains valid states for the <see cref="BuildingController"/>
    /// </summary>
    struct BuildingState
    {
        public const string closed = "closed";
        public const string outOfHours = "out of hours";
        public const string open = "open";
        public const string fireDrill = "fire drill";
        public const string fireAlarm = "fire alarm";
    }

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
                    Assert.AreEqual(argName, ControllerArgNames.buildingID);
                }
            }
        }

        /// <summary>
        /// Test initialisation of <c>buildingID</c> when constructor parameter set.
        /// Satisfies <req>L1R2</req>, <req>L1R3</req>.
        /// </summary>
        [TestCase("")]
        [TestCase("Building ID")]
        public void Constructor_WhenSet_InitialisesBuildingID(string buildingID)
        {
            BuildingController controller;

            controller = new BuildingController(buildingID);
            string result = controller.GetBuildingID();

            Assert.AreEqual(buildingID.ToLower(), result);
        }

        /// <summary>
        /// Test <c>buildingID</c> setter.
        /// Satisfies <req>L1R4</req>.
        /// </summary>
        [TestCase("Building ID")]
        public void SetBuildingID_WhenSet_SetsID(string buildingID)
        {
            BuildingController controller;

            controller = new BuildingController("");
            controller.SetBuildingID(buildingID);
            string result = controller.GetBuildingID();

            Assert.AreEqual(buildingID.ToLower(), result);
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

            Assert.AreEqual(BuildingState.outOfHours, result);
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

            Assert.IsTrue(result);
        }

        /// <summary>
        /// Test <c>currentState</c> setter with valid states.
        /// Satisfies <req>L1R7</req>.
        /// </summary>
        [Test, TestCaseSource(nameof(ValidBuildingStates))]
        public void SetCurrentState_WhenValidState_SetsState(string state)
        {
            BuildingController controller;

            controller = new BuildingController("");
            controller.SetCurrentState(state);
            string result = controller.GetCurrentState();

            Assert.AreEqual(state, result);
        }

        /// <summary>
        /// Test <c>currentState</c> setter with invalid states.
        /// Satisfies <req>L1R7</req>.
        /// </summary>
        [Test, TestCaseSource(nameof(InvalidBuildingStates))]
        public void SetCurrentState_WhenInvalidState_ReturnsFalse(string state)
        {
            BuildingController controller;

            controller = new BuildingController("");
            bool result = controller.SetCurrentState(state);

            Assert.IsFalse(result);
        }

        /// <summary>
        /// Test <c>currentState</c> setter with invalid states.
        /// Satisfies <req>L1R7</req>.
        /// </summary>
        [Test, TestCaseSource(nameof(InvalidBuildingStates))]
        public void SetCurrentState_WhenInvalidState_DoesNotSetState(string state)
        {
            BuildingController controller;

            controller = new BuildingController("");
            controller.SetCurrentState(state);
            string result = controller.GetCurrentState();

            Assert.AreEqual(BuildingState.outOfHours, result);
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

            Assert.AreEqual(success, result);
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

            Assert.AreEqual(success, result);
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

            Assert.AreEqual(state, result);
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

            Assert.AreEqual(state, result);
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

            Assert.AreEqual(BuildingState.closed, result);
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

            Assert.AreEqual(BuildingState.open, result);
        }

        // Emergency States

        /// <summary>
        /// Test <c>currentState</c> setter when transitioning from a 'fire alarm' state to the previous one.
        /// Satisfies <req>L2R1</req>.
        /// </summary>
        [Test, TestCaseSource(nameof(NormalBuildingStates))]
        public void SetCurrentState_WhenCurrentStateAlarmAndPreviousState_ReturnsTrue(string state)
        {
            BuildingController controller;

            controller = new BuildingController("");
            controller.SetCurrentState(state);
            controller.SetCurrentState(BuildingState.fireAlarm);
            bool result = controller.SetCurrentState(state);
            string newState = controller.GetCurrentState();

            Assert.IsTrue(result);
            Assert.AreEqual(state, newState);
        }

        /// <summary>
        /// Test <c>currentState</c> setter when transitioning from a 'fire drill' state to the previous one.
        /// Satisfies <req>L2R1</req>.
        /// </summary>
        [Test, TestCaseSource(nameof(NormalBuildingStates))]
        public void SetCurrentState_WhenCurrentStateDrillAndPreviousState_ReturnsTrue(string state)
        {
            BuildingController controller;

            controller = new BuildingController("");
            controller.SetCurrentState(state);
            controller.SetCurrentState(BuildingState.fireDrill);
            bool result = controller.SetCurrentState(state);
            string newState = controller.GetCurrentState();

            Assert.IsTrue(result);
            Assert.AreEqual(state, newState);
        }

        /// <summary>
        /// Test <c>currentState</c> setter when transitioning from a 'fire alarm' state to one different from the previous.
        /// Satisfies <req>L2R1</req>.
        /// </summary>
        [Test, TestCaseSource(nameof(NormalBuildingStates))]
        public void SetCurrentState_WhenCurrentStateAlarmAndNotPreviousState_ReturnsFalse(string state)
        {
            BuildingController controller;

            controller = new BuildingController("");
            controller.SetCurrentState(state);
            controller.SetCurrentState(BuildingState.fireAlarm);

            bool stateHasChanged = false;

            foreach (string otherState in ValidBuildingStates)
            {
                if (otherState != state)
                {
                    bool stateChange = controller.SetCurrentState(state);

                    if (stateChange)
                    {
                        stateHasChanged = true;
                    }
                }
            }

            string result = controller.GetCurrentState();

            Assert.AreEqual(state, result);
            Assert.IsFalse(stateHasChanged);
        }

        /// <summary>
        /// Test <c>currentState</c> setter when transitioning from a 'fire drill' state to one different from the previous.
        /// Satisfies <req>L2R1</req>.
        /// </summary>
        [Test, TestCaseSource(nameof(NormalBuildingStates))]
        public void SetCurrentState_WhenCurrentStateDrillAndNotPreviousState_ReturnsFalse(string state)
        {
            BuildingController controller;

            controller = new BuildingController("");
            controller.SetCurrentState(state);
            controller.SetCurrentState(BuildingState.fireDrill);

            bool stateHasChanged = false;

            foreach (string otherState in ValidBuildingStates)
            {
                if (otherState != state)
                {
                    bool stateChange = controller.SetCurrentState(state);

                    if (stateChange)
                    {
                        stateHasChanged = true;
                    }
                }
            }

            string result = controller.GetCurrentState();

            Assert.AreEqual(state, result);
            Assert.IsFalse(stateHasChanged);
        }

        // L2R2 (SetCurrentState when same state)

        /// <summary>
        /// Test <c>currentState</c> setter when the state is the same.
        /// Satisfies <req>L2R2</req>.
        /// </summary>
        [Test, TestCaseSource(nameof(ValidBuildingStates))]
        public void SetCurrentState_WhenSameState_ReturnsTrue(string state)
        {
            BuildingController controller;

            controller = new BuildingController("");
            controller.SetCurrentState(state);

            bool result = controller.SetCurrentState(state);
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Test <c>currentState</c> setter when the state is the same.
        /// Satisfies <req>L2R2</req>.
        /// </summary>
        [Test, TestCaseSource(nameof(ValidBuildingStates))]
        public void SetCurrentState_WhenSameState_RetainsState(string state)
        {
            BuildingController controller;

            controller = new BuildingController("");
            controller.SetCurrentState(state);
            controller.SetCurrentState(state);

            string result = controller.GetCurrentState();
            Assert.AreEqual(state, result);
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

                    Assert.AreEqual(firstArgName, ControllerArgNames.buildingID);
                    Assert.AreEqual(secondArgName, ControllerArgNames.startState);
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
            Assert.AreEqual(state, result);
        }

        /// <summary>
        /// Test constructor when using startState argument with a normal state in capital letters.
        /// Satisfies <req>L2R3</req>.
        /// </summary>
        [Test, TestCaseSource(nameof(NormalBuildingStates))]
        public void Constructor_WhenNormalStateCapitals_SetsStartState(string state)
        {
            BuildingController controller;

            controller = new BuildingController("", state.ToUpper());

            string result = controller.GetCurrentState();
            Assert.AreEqual(state, result);
        }

        /// <summary>
        /// Test constructor when using startState argument with a normal state in title case.
        /// Satisfies <req>L2R3</req>.
        /// </summary>
        [Test, TestCaseSource(nameof(NormalBuildingStates))]
        public void Constructor_WhenNormalStateMixedCapitals_SetsStartState(string state)
        {
            BuildingController controller;

            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
            controller = new BuildingController("", ti.ToTitleCase(state));

            string result = controller.GetCurrentState();
            Assert.AreEqual(state, result);
        }

        /// <summary>
        /// Test constructor when using startState argument with an emergency / invalid state.
        /// Satisfies <req>L2R3</req>.
        /// </summary>
        [TestCase(BuildingState.fireDrill)]
        [TestCase(BuildingState.fireAlarm)]
        [TestCaseSource(nameof(InvalidBuildingStates))]
        public void Constructor_WhenEmergencyState_ThrowsException(string state)
        {
            BuildingController controller;

            ArgumentException controllerException = Assert.Throws<ArgumentException>(() => {
                controller = new BuildingController("", state);
            });

            Assert.AreEqual(ControllerExceptions.initialStateException, controllerException.Message);
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
                Assume.That(constructorParams.Length, Is.EqualTo(argNames.Length));

                bool parameterNamesMatch = true;

                for (int i = 0; i < constructorParams.Length; i++)
                {
                    ParameterInfo parameter = constructorParams.ElementAt(i);
                    string paramName = parameter.Name == null ? "" : parameter.Name;

                    if (paramName == argNames.ElementAt(i))
                    {
                        parameterNamesMatch = false;
                    }
                }

                Assert.IsTrue(parameterNamesMatch);
            }
        }
    }
}