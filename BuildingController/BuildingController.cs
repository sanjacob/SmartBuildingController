using System.Linq;

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
            /// Hold all normal states as an array.
            /// </summary>
            private static string[] normalStates = new string[] { closed, open, outOfHours };

            public static Dictionary<string, string> forbiddenTransition = new(){
                {closed, open},
                {open, closed}
            };

            public static bool isValid(string state)
            {
                return validStates.Contains(state);
            }

            public static bool isNormal(string state)
            {
                return normalStates.Contains(state);
            }

            public static bool isEmergency(string state)
            {
                return state == fireAlarm || state == fireDrill;
            }
        }

        private class ErrorMessages
        {
            public const string invalidStartState = "Argument Exception: BuildingController can only be initialised "
                + "to the following states 'open', 'closed', 'out of hours'";
        }

        private string buildingID;
        private string currentState;
        private string pastState = State.outOfHours;

        private ILightManager? lightManager;
        private IFireAlarmManager? fireAlarmManager;
        private IDoorManager? doorManager;
        private IWebService? webService;
        private IEmailService? emailService;

        const string okDevice = "OK";
        const string faultyDevice = "FAULT";

        struct Manager
        {
            public const string lights = "Lights";
            public const string doors = "Doors";
            public const string alarm = "FireAlarm";
        }

        public BuildingController(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                id = id.ToLower();
            }

            buildingID = id;
            currentState = InitialState();
        }

        public BuildingController(string id, string startState)
        {
            buildingID = id.ToLower();
            currentState = InitialState(startState);
        }

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

        private string InitialState(string state = State.initialState)
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

        public string GetBuildingID()
        {
            return buildingID;
        }

        public string GetCurrentState()
        {
            return currentState;
        }

        public string GetStatusReport()
        {
            string report = "";

            if (lightManager != null && doorManager != null && fireAlarmManager != null)
            {
                string lightStatus = lightManager.GetStatus();
                string doorStatus = doorManager.GetStatus();
                string fireAlarmStatus = fireAlarmManager.GetStatus();

                List<string> faultyDevices = new List<string>();
                
                if (ParseStatus(lightStatus, Manager.lights))
                {
                    faultyDevices.Add(Manager.lights);
                }

                if (ParseStatus(doorStatus, Manager.doors))
                {
                    faultyDevices.Add(Manager.doors);
                }

                if (ParseStatus(fireAlarmStatus, Manager.alarm))
                {
                    faultyDevices.Add(Manager.alarm);
                }


                if (faultyDevices.Count > 0)
                {

                    string webLog = string.Concat(string.Join(',', faultyDevices.ToArray()), ",");

                    if (faultyDevices.Count == 1)
                    {
                        webLog = faultyDevices.First();
                    }


                    if (webService != null)
                    {
                        webService.LogEngineerRequired(webLog);
                    }
                }
                
                report = string.Format("{0}{1}{2}", lightStatus, doorStatus, fireAlarmStatus);
            }

            return report;
        }

        /// <summary>
        /// Parse the status of a manager
        /// </summary>
        /// <param name="status">Status returned by a manager</param>
        /// <returns></returns>
        protected bool ParseStatus(string status, string managerType) {
            bool faulty = true;

            if (!string.IsNullOrEmpty(status))
            {
                string[] devices = status.Split(',');

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

        public void SetBuildingID(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                id = id.ToLower();
            }

            buildingID = id;
        }

        public bool SetCurrentState(string state)
        {
            bool validTransition = false;
            if (!string.IsNullOrEmpty(state))
            {
                state = state.ToLower();
                validTransition = IsValidStateTransition(state);

                if (state != currentState)
                {
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

        protected bool StateTransitionSideEffects(string state)
        {
            bool validTransition = true;

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