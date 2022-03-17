namespace SmartBuilding
{
    /// <summary>
    /// Controller for the UCLan Smart Building.
    /// </summary>
    public class BuildingController : IBuildingController
    {
        /// <summary>
        /// Hold valid building states.
        /// </summary>
        private class State
        {
            public const string closed = "closed";
            public const string open = "open";
            public const string outOfHours = "out of hours";
            public const string fireAlarm = "fire alarm";
            public const string fireDrill = "fire drill";
            public const string initialState = outOfHours;

            /// <summary>
            /// Hold all valid states as an array.
            /// </summary>
            private static string[] validStates = new string[] { closed, open, outOfHours, fireAlarm, fireDrill };

            /// <summary>
            /// Hold all non-emergency states as an array.
            /// </summary>
            private static string[] normalStates = new string[] { closed, open, outOfHours };

            /// <summary>
            /// Holds forbidden transitions for certain states.
            /// </summary>
            public static Dictionary<string, string> forbiddenTransition = new(){
                {closed, open},
                {open, closed}
            };

            /// <summary>
            /// Whether a state is within the acceptable values.
            /// </summary>
            /// <param name="state">State to check.</param>
            public static bool isValid(string state)
            {
                return validStates.Contains(state);
            }

            /// <summary>
            /// Whether a state is a non-emergency one.
            /// </summary>
            /// <param name="state">State to check.</param>
            public static bool isNormal(string state)
            {
                return normalStates.Contains(state);
            }

            /// <summary>
            /// Whether state is an emergency state (or drill).
            /// </summary>
            /// <param name="state">State to check.</param>
            public static bool isEmergency(string state)
            {
                return state == fireAlarm || state == fireDrill;
            }
        }

        /// <summary>
        /// Controller exception messages.
        /// </summary>
        private class ErrorMessages
        {
            public const string invalidStartState = "Argument Exception: BuildingController can only be initialised "
                + "to the following states 'open', 'closed', 'out of hours'";
        }

        /// <summary>
        /// Stores the building ID
        /// </summary>
        private string buildingID;
        /// <summary>
        /// Stores the current building state
        /// </summary>
        private string currentState;
        /// <summary>
        /// Keep record of previous state
        /// </summary>
        private string pastState = State.outOfHours;

        // Managers
        private ILightManager? lightManager;
        private IFireAlarmManager? fireAlarmManager;
        private IDoorManager? doorManager;
        // Services
        private IWebService? webService;
        private IEmailService? emailService;

        // String returned by a device working correctly
        const string okDevice = "OK";
        // String returned by a faulty device
        const string faultyDevice = "FAULT";

        /// <summary>
        /// Manager names.
        /// </summary>
        struct Manager
        {
            public const string lights = "Lights";
            public const string doors = "Doors";
            public const string alarm = "FireAlarm";
        }

        /// <summary>
        /// Create a BuildingController with the supplied id.
        /// Any uppercase chars will be converted to lower case.
        /// </summary>
        /// <param name="id">The building id</param>
        public BuildingController(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                id = id.ToLower();
            }

            buildingID = id;
            currentState = InitialState();
        }

        /// <summary>
        /// Instantiate a BuildingController with a specified initial state.
        /// </summary>
        /// <param name="id">The building id</param>
        /// <param name="startState">The initial state of the building</param>
        public BuildingController(string id, string startState)
        {
            buildingID = id.ToLower();
            currentState = InitialState(startState);
        }

        /// <summary>
        /// Create a BuildingController with the specified managers and services.
        /// </summary>
        /// <param name="id">The building id</param>
        /// <param name="iLightManager"></param>
        /// <param name="iFireAlarmManager"></param>
        /// <param name="iDoorManager"></param>
        /// <param name="iWebService"></param>
        /// <param name="iEmailService"></param>
        public BuildingController(string id, ILightManager iLightManager, IFireAlarmManager iFireAlarmManager,
            IDoorManager iDoorManager, IWebService iWebService, IEmailService iEmailService)
        {
            buildingID = id.ToLower();
            currentState = InitialState();

            lightManager = iLightManager;
            fireAlarmManager = iFireAlarmManager;
            doorManager = iDoorManager;
            webService = iWebService;
            emailService = iEmailService;
        }

        /// <summary>
        /// Validate and return the system's initial state.
        /// </summary>
        /// <param name="state">The building's initial state</param>
        /// <returns>The building's initial state.</returns>
        /// <exception cref="ArgumentException">Thrown if the supplied state is not a valid normal building state.</exception>
        protected string InitialState(string state = State.initialState)
        {
            bool validState = !string.IsNullOrEmpty(state);

            if (validState)
            {
                state = state.ToLower();
                validState = State.isNormal(state);
            }

            if (!validState)
            {
                throw new ArgumentException(ErrorMessages.invalidStartState);
            }

            return state;            
        }

        /// <summary>
        /// Get the building ID.
        /// </summary>        
        /// <returns>Building ID</returns>

        public string GetBuildingID()
        {
            return buildingID;
        }

        /// <summary>
        /// Get the building's current state.
        /// </summary>
        /// <returns>Current state</returns>
        public string GetCurrentState()
        {
            return currentState;
        }

        /// <summary>
        /// Get the status report for all managers.
        /// </summary>
        /// <returns>A string containing the status report of the <see cref="IDoorManager"/>, <see cref="ILightManager"/>, and <see cref="IFireAlarmManager"/>.</returns>
        public string GetStatusReport()
        {
            string report = "";

            // Only create report if managers are not null
            if (lightManager != null && doorManager != null && fireAlarmManager != null)
            {
                string lightStatus = lightManager.GetStatus();
                string doorStatus = doorManager.GetStatus();
                string fireAlarmStatus = fireAlarmManager.GetStatus();

                List<string> faultyDevices = new List<string>();
                
                // Parse device statuses for faults
                if (ParseStatus(lightStatus, Manager.lights))
                {
                    faultyDevices.Add(Manager.lights);
                }

                if (ParseStatus(fireAlarmStatus, Manager.alarm))
                {
                    faultyDevices.Add(Manager.alarm);
                }

                if (ParseStatus(doorStatus, Manager.doors))
                {
                    faultyDevices.Add(Manager.doors);
                }


                // Log faulty devices
                if (faultyDevices.Count > 0)
                {
                    // Join device names with a comma
                    string webLog = string.Concat(string.Join(',', faultyDevices.ToArray()), ",");

                    // Or just a single device name
                    if (faultyDevices.Count == 1)
                    {
                        webLog = faultyDevices.First();
                    }

                    if (webService != null)
                    {
                        webService.LogEngineerRequired(webLog);
                    }
                }
                
                report = string.Format("{0}{1}{2}", lightStatus, fireAlarmStatus, doorStatus);
            }

            return report;
        }

        /// <summary>
        /// Parse the status of a manager
        /// </summary>
        /// <param name="status">Status returned by a manager</param>
        /// <param name="managerType">Status returned by a manager</param>
        /// <returns>True if status was incorrect or faulty, otherwise false.</returns>
        protected bool ParseStatus(string status, string managerType) {
            bool faulty = true;

            if (!string.IsNullOrEmpty(status))
            {
                string[] devices = status.Split(',');

                // First element of array must be manager name
                // Last one must be empty, because of the trailing comma
                if (devices.First() == managerType && devices.Last() == "")
                {
                    faulty = false;

                    for (int i = 1; i < devices.Length - 1; i++)
                    {
                        if (devices.ElementAt(i) != okDevice)
                        {
                            faulty = true;
                        }
                    }
                }
            }
            
            return faulty;
        }

        /// <summary>
        /// Set the Building ID.
        /// </summary>
        /// <param name="id">New building ID</param>
        public void SetBuildingID(string id)
        {
            // If possible, convert string to lowercase
            if (!string.IsNullOrEmpty(id))
            {
                id = id.ToLower();
            }

            buildingID = id;
        }

        /// <summary>
        /// Set a new state for the building.
        /// </summary>
        /// <param name="state">State to transition to</param>
        /// <returns>Whether transition was allowed.</returns>
        public bool SetCurrentState(string state)
        {
            bool validTransition = false;
            if (!string.IsNullOrEmpty(state))
            {
                state = state.ToLower();
                validTransition = IsValidStateTransition(state);

                if (state != currentState)
                {
                    // Apply manager operations
                    validTransition &= StateTransitionSideEffects(state);

                    if (validTransition)
                    {
                        // Remember and update currentState
                        pastState = currentState;
                        currentState = state;
                    }
                }
            }

            return validTransition;
        }

        /// <summary>
        /// Verify if a state transition is allowed.
        /// </summary>
        /// <param name="state">The state to transition to.</param>
        /// <returns>Whether transition is valid.</returns>
        protected bool IsValidStateTransition(string state)
        {
            bool validTransition = false;

            // Only consider valid states
            if (State.isValid(state))
            {
                validTransition = true;

                if (state != currentState)
                {
                    if (State.isNormal(currentState))
                    {
                        // Disallow transition from open to close or viceversa
                        if (State.forbiddenTransition.ContainsKey(currentState))
                        {
                            validTransition = State.forbiddenTransition[currentState] != state;
                        }
                    }
                    else if (State.isEmergency(currentState))
                    {
                        // New state matches state at the time of entering emergency state.
                        validTransition = (state == pastState);
                    }
                }
            }

            return validTransition;
        }

        /// <summary>
        /// Calls manager methods when changing states.
        /// </summary>
        /// <param name="state">State to transition to.</param>
        /// <returns>Whether transition should be allowed.</returns>
        protected bool StateTransitionSideEffects(string state)
        {
            bool validTransition = true;

            // Handle different transitions with required manager operations
            if (state == State.open)
            {
                if (doorManager != null)
                {
                    if (!doorManager.OpenAllDoors())
                    {
                        validTransition = false;
                    }
                }
            }
            else if (state == State.closed)
            {
                if (doorManager != null)
                {
                    doorManager.LockAllDoors();
                }

                if (lightManager != null)
                {
                    lightManager.SetAllLights(false);
                }
            }
            else if (state == State.fireAlarm)
            {
                if (fireAlarmManager != null)
                {
                    fireAlarmManager.SetAlarm(true);
                }

                if (doorManager != null)
                {
                    doorManager.OpenAllDoors();
                }

                if (lightManager != null)
                {
                    lightManager.SetAllLights(true);
                }

                if (webService != null)
                {
                    webService.LogFireAlarm(State.fireAlarm);
                }
            }

            return validTransition;
        }
    }
}