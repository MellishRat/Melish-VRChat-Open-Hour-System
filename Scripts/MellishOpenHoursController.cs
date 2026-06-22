using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

namespace Mellish.OpenHours
{
    /// <summary>
    /// Mellish's VRChat Open Hour System
    /// Lightweight UdonSharp controller for showing/hiding world objects based on scheduled opening hours.
    /// Designed for VRChat worlds using Unity 2022.3.x and VRChat World SDK 3.10.x.
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class MellishOpenHoursController : UdonSharpBehaviour
    {
        [Header("Objects To Toggle")]
        [Tooltip("Objects that are enabled while the venue/feature is open.")]
        public GameObject[] objectsVisibleWhenOpen;

        [Tooltip("Objects that are enabled while the venue/feature is closed.")]
        public GameObject[] objectsVisibleWhenClosed;

        [Header("Status Display")]
        [Tooltip("Optional UI Text that displays the current open/closed status.")]
        public Text statusText;

        [Tooltip("Message shown while open.")]
        public string openMessage = "OPEN";

        [Tooltip("Message shown while closed.")]
        public string closedMessage = "CLOSED";

        [Tooltip("Text colour used while open.")]
        public Color openTextColour = Color.green;

        [Tooltip("Text colour used while closed.")]
        public Color closedTextColour = Color.red;

        [Header("Countdown Display")]
        [Tooltip("Optional UI Text that displays time until opening or closing.")]
        public Text countdownText;

        [Tooltip("Prefix shown while closed, before the countdown.")]
        public string opensInPrefix = "Opens In:";

        [Tooltip("Prefix shown while open, before the countdown.")]
        public string closesInPrefix = "Closes In:";

        [Tooltip("Show countdown while the venue/feature is open.")]
        public bool showCountdownWhileOpen = true;

        [Tooltip("Show countdown while the venue/feature is closed.")]
        public bool showCountdownWhileClosed = true;

        [Tooltip("Show the next opening day and time while closed.")]
        public bool showNextOpeningTime = true;

        [Header("Audio Cues")]
        [Tooltip("Optional AudioSource used to play the opening/closing sounds.")]
        public AudioSource audioSource;

        [Tooltip("Played once when the state changes from closed to open.")]
        public AudioClip openingSound;

        [Tooltip("Played once when the state changes from open to closed.")]
        public AudioClip closingSound;

        [Tooltip("If enabled, the matching sound can play on the first refresh after world load.")]
        public bool playSoundOnFirstRefresh = false;

        [Header("Status Light")]
        [Tooltip("Optional light used as a visual open/closed indicator.")]
        public Light statusLight;

        [Tooltip("If enabled, the light is only active while open. If disabled, it stays on and changes colour.")]
        public bool onlyEnableLightWhenOpen = false;

        [Tooltip("Light colour used while open.")]
        public Color openLightColour = Color.green;

        [Tooltip("Light colour used while closed.")]
        public Color closedLightColour = Color.red;

        [Header("Status Mesh Material")]
        [Tooltip("Optional renderer for a physical sign, bulb, lens, or mesh indicator that should change material with the open/closed state.")]
        public Renderer statusMeshRenderer;

        [Tooltip("Material used on the status mesh while open.")]
        public Material openMeshMaterial;

        [Tooltip("Material used on the status mesh while closed.")]
        public Material closedMeshMaterial;

        [Tooltip("If enabled, the status mesh renderer is only visible while open. If disabled, it stays visible and swaps materials.")]
        public bool onlyShowMeshWhenOpen = false;

        [Header("Reference Time Zone")]
        [Tooltip("Schedule time zone offset from UTC. Examples: UK winter = 0, Japan = 9, US Eastern winter = -5, Germany winter = 1.")]
        [Range(-12, 14)] public int utcOffsetHours = 0;

        [Tooltip("Optional extra minutes for time zones with half-hour offsets. Usually leave this at 0.")]
        [Range(-59, 59)] public int utcOffsetMinutes = 0;

        [Header("Opening Hours - Reference Time")]
        [Tooltip("Opening hour in 24-hour reference time. Example: 22 for 10 PM.")]
        [Range(0, 23)] public int openingHour = 0;

        [Tooltip("Opening minute in reference time.")]
        [Range(0, 59)] public int openingMinute = 0;

        [Tooltip("Closing hour in 24-hour reference time. Overnight ranges are supported, such as 22:00 to 02:00.")]
        [Range(0, 23)] public int closingHour = 23;

        [Tooltip("Closing minute in reference time.")]
        [Range(0, 59)] public int closingMinute = 59;

        [Header("Open Days")]
        public bool openMonday = true;
        public bool openTuesday = true;
        public bool openWednesday = true;
        public bool openThursday = true;
        public bool openFriday = true;
        public bool openSaturday = true;
        public bool openSunday = true;

        [Header("Update Settings")]
        [Tooltip("How often the schedule checks itself, in seconds. 30-60 seconds is normally enough.")]
        [Range(5f, 3600f)] public float checkIntervalSeconds = 60f;

        [Tooltip("Disable this if you only want to call RefreshNow manually from another Udon behaviour.")]
        public bool autoRefresh = true;

        [Header("Debug")]
        [Tooltip("Writes useful schedule information to the optional debug text.")]
        public bool showDebug = false;

        [Tooltip("Optional UI Text used for debug output.")]
        public Text debugText;

        private const int MinutesPerDay = 1440;
        private const int MinutesPerWeek = 10080;

        private bool _hasKnownState = false;
        private bool _lastOpenState = false;

        private void Start()
        {
            RefreshNow();

            if (autoRefresh)
            {
                SendCustomEventDelayedSeconds(nameof(RefreshLoop), checkIntervalSeconds);
            }
        }

        public void RefreshLoop()
        {
            RefreshNow();

            if (autoRefresh)
            {
                SendCustomEventDelayedSeconds(nameof(RefreshLoop), checkIntervalSeconds);
            }
        }

        /// <summary>
        /// Public refresh method, useful if another Udon script or button needs to force a schedule update.
        /// </summary>
        public void RefreshNow()
        {
            DateTime networkUtcTime = Networking.GetNetworkDateTime();
            DateTime referenceTime = GetReferenceTime(networkUtcTime);
            bool isOpen = IsOpenAtTime(referenceTime);

            ApplyState(isOpen, networkUtcTime, referenceTime);
        }

        private DateTime GetReferenceTime(DateTime networkUtcTime)
        {
            return networkUtcTime.AddHours(utcOffsetHours).AddMinutes(utcOffsetMinutes);
        }

        private bool IsOpenAtTime(DateTime currentTime)
        {
            int today = (int)currentTime.DayOfWeek; // Sunday = 0, Monday = 1, etc.
            int yesterday = today - 1;
            if (yesterday < 0) yesterday = 6;

            int currentMinutes = GetTimeMinutes(currentTime.Hour, currentTime.Minute);
            int openMinutes = GetTimeMinutes(openingHour, openingMinute);
            int closeMinutes = GetTimeMinutes(closingHour, closingMinute);

            bool normalSameDayRange = openMinutes <= closeMinutes;

            if (normalSameDayRange)
            {
                return IsDayEnabled(today) && currentMinutes >= openMinutes && currentMinutes < closeMinutes;
            }

            // Overnight range, for example Friday 22:00 to Saturday 02:00.
            bool openLateToday = IsDayEnabled(today) && currentMinutes >= openMinutes;
            bool stillOpenFromYesterday = IsDayEnabled(yesterday) && currentMinutes < closeMinutes;

            return openLateToday || stillOpenFromYesterday;
        }

        private bool IsDayEnabled(int day)
        {
            if (day == 0) return openSunday;
            if (day == 1) return openMonday;
            if (day == 2) return openTuesday;
            if (day == 3) return openWednesday;
            if (day == 4) return openThursday;
            if (day == 5) return openFriday;
            if (day == 6) return openSaturday;
            return false;
        }

        private void ApplyState(bool isOpen, DateTime networkUtcTime, DateTime referenceTime)
        {
            SetObjectsActive(objectsVisibleWhenOpen, isOpen);
            SetObjectsActive(objectsVisibleWhenClosed, !isOpen);
            UpdateStatusText(isOpen);
            UpdateCountdownText(isOpen, referenceTime);
            UpdateStatusLight(isOpen);
            UpdateStatusMeshMaterial(isOpen);
            PlayStateSoundIfNeeded(isOpen);
            UpdateDebugText(isOpen, networkUtcTime, referenceTime);

            _lastOpenState = isOpen;
            _hasKnownState = true;
        }

        private void SetObjectsActive(GameObject[] objectsToSet, bool active)
        {
            if (objectsToSet == null) return;

            for (int i = 0; i < objectsToSet.Length; i++)
            {
                GameObject target = objectsToSet[i];
                if (target == null) continue;

                if (target.activeSelf != active)
                {
                    target.SetActive(active);
                }
            }
        }

        private void UpdateStatusText(bool isOpen)
        {
            if (statusText == null) return;

            statusText.text = isOpen ? openMessage : closedMessage;
            statusText.color = isOpen ? openTextColour : closedTextColour;
        }

        private void UpdateCountdownText(bool isOpen, DateTime referenceTime)
        {
            if (countdownText == null) return;

            if (isOpen && !showCountdownWhileOpen)
            {
                countdownText.text = string.Empty;
                return;
            }

            if (!isOpen && !showCountdownWhileClosed)
            {
                countdownText.text = string.Empty;
                return;
            }

            int minutesUntil = isOpen ? GetMinutesUntilClose(referenceTime) : GetMinutesUntilOpen(referenceTime);

            if (minutesUntil < 0)
            {
                countdownText.text = string.Empty;
                return;
            }

            string prefix = isOpen ? closesInPrefix : opensInPrefix;
            string countdown = FormatDuration(minutesUntil);

            if (!isOpen && showNextOpeningTime)
            {
                string nextOpening = GetNextOpeningLabel(referenceTime);
                countdownText.text = "Next Opening: " + nextOpening + "\n" + prefix + "\n" + countdown;
            }
            else
            {
                countdownText.text = prefix + "\n" + countdown;
            }
        }

        private void UpdateStatusLight(bool isOpen)
        {
            if (statusLight == null) return;

            statusLight.color = isOpen ? openLightColour : closedLightColour;

            if (onlyEnableLightWhenOpen)
            {
                statusLight.enabled = isOpen;
            }
            else if (!statusLight.enabled)
            {
                statusLight.enabled = true;
            }
        }

        private void UpdateStatusMeshMaterial(bool isOpen)
        {
            if (statusMeshRenderer == null) return;

            Material targetMaterial = isOpen ? openMeshMaterial : closedMeshMaterial;
            if (targetMaterial != null)
            {
                statusMeshRenderer.material = targetMaterial;
            }

            if (onlyShowMeshWhenOpen)
            {
                statusMeshRenderer.enabled = isOpen;
            }
            else if (!statusMeshRenderer.enabled)
            {
                statusMeshRenderer.enabled = true;
            }
        }

        private void PlayStateSoundIfNeeded(bool isOpen)
        {
            if (audioSource == null) return;

            bool stateChanged = !_hasKnownState || _lastOpenState != isOpen;
            if (!stateChanged) return;

            if (!_hasKnownState && !playSoundOnFirstRefresh) return;

            AudioClip clip = isOpen ? openingSound : closingSound;
            if (clip == null) return;

            audioSource.PlayOneShot(clip);
        }

        private void UpdateDebugText(bool isOpen, DateTime networkUtcTime, DateTime referenceTime)
        {
            if (debugText == null) return;

            if (!showDebug)
            {
                debugText.text = string.Empty;
                return;
            }

            string state = isOpen ? "OPEN" : "CLOSED";
            string utcTime = Pad2(networkUtcTime.Hour) + ":" + Pad2(networkUtcTime.Minute);
            string refTime = Pad2(referenceTime.Hour) + ":" + Pad2(referenceTime.Minute);
            string openTime = Pad2(openingHour) + ":" + Pad2(openingMinute);
            string closeTime = Pad2(closingHour) + ":" + Pad2(closingMinute);
            string offset = FormatUtcOffset();
            int countdownMinutes = isOpen ? GetMinutesUntilClose(referenceTime) : GetMinutesUntilOpen(referenceTime);
            string countdownLabel = countdownMinutes >= 0 ? FormatDuration(countdownMinutes) : "No enabled open days";

            debugText.text =
                "Mellish Open Hours Debug\n" +
                "State: " + state + "\n" +
                "Network UTC/GMT: " + networkUtcTime.DayOfWeek + " " + utcTime + "\n" +
                "Reference Time: " + referenceTime.DayOfWeek + " " + refTime + " " + offset + "\n" +
                "Open Range: " + openTime + " - " + closeTime + "\n" +
                (isOpen ? "Closes In: " : "Opens In: ") + countdownLabel + "\n" +
                "Auto Refresh: " + autoRefresh + " / " + checkIntervalSeconds + "s";
        }

        private int GetMinutesUntilOpen(DateTime referenceTime)
        {
            int currentWeekMinute = GetWeekMinute(referenceTime);
            int openMinuteOfDay = GetTimeMinutes(openingHour, openingMinute);
            int bestDelta = -1;

            for (int offsetDays = 0; offsetDays < 7; offsetDays++)
            {
                DateTime candidateDay = referenceTime.Date.AddDays(offsetDays);
                int day = (int)candidateDay.DayOfWeek;
                if (!IsDayEnabled(day)) continue;

                int candidateWeekMinute = (day * MinutesPerDay) + openMinuteOfDay;
                int delta = candidateWeekMinute - currentWeekMinute;
                if (delta < 0) delta += MinutesPerWeek;

                if (delta == 0 && !IsOpenAtTime(referenceTime))
                {
                    delta = MinutesPerWeek;
                }

                if (bestDelta < 0 || delta < bestDelta)
                {
                    bestDelta = delta;
                }
            }

            return bestDelta;
        }

        private int GetMinutesUntilClose(DateTime referenceTime)
        {
            int today = (int)referenceTime.DayOfWeek;
            int yesterday = today - 1;
            if (yesterday < 0) yesterday = 6;

            int currentMinutes = GetTimeMinutes(referenceTime.Hour, referenceTime.Minute);
            int openMinutes = GetTimeMinutes(openingHour, openingMinute);
            int closeMinutes = GetTimeMinutes(closingHour, closingMinute);
            bool normalSameDayRange = openMinutes <= closeMinutes;

            if (normalSameDayRange)
            {
                if (IsDayEnabled(today) && currentMinutes >= openMinutes && currentMinutes < closeMinutes)
                {
                    return closeMinutes - currentMinutes;
                }

                return -1;
            }

            // Overnight schedule.
            if (IsDayEnabled(today) && currentMinutes >= openMinutes)
            {
                return (MinutesPerDay - currentMinutes) + closeMinutes;
            }

            if (IsDayEnabled(yesterday) && currentMinutes < closeMinutes)
            {
                return closeMinutes - currentMinutes;
            }

            return -1;
        }

        private string GetNextOpeningLabel(DateTime referenceTime)
        {
            int currentWeekMinute = GetWeekMinute(referenceTime);
            int openMinuteOfDay = GetTimeMinutes(openingHour, openingMinute);
            int bestDelta = -1;
            int bestDay = -1;

            for (int offsetDays = 0; offsetDays < 7; offsetDays++)
            {
                DateTime candidateDay = referenceTime.Date.AddDays(offsetDays);
                int day = (int)candidateDay.DayOfWeek;
                if (!IsDayEnabled(day)) continue;

                int candidateWeekMinute = (day * MinutesPerDay) + openMinuteOfDay;
                int delta = candidateWeekMinute - currentWeekMinute;
                if (delta < 0) delta += MinutesPerWeek;

                if (bestDelta < 0 || delta < bestDelta)
                {
                    bestDelta = delta;
                    bestDay = day;
                }
            }

            if (bestDay < 0) return "Not scheduled";

            return DayName(bestDay) + " " + Pad2(openingHour) + ":" + Pad2(openingMinute) + " " + FormatUtcOffset();
        }

        private int GetWeekMinute(DateTime time)
        {
            int day = (int)time.DayOfWeek;
            return (day * MinutesPerDay) + GetTimeMinutes(time.Hour, time.Minute);
        }

        private int GetTimeMinutes(int hour, int minute)
        {
            return (hour * 60) + minute;
        }

        private string FormatDuration(int totalMinutes)
        {
            if (totalMinutes < 0) return "Not scheduled";

            int hours = totalMinutes / 60;
            int minutes = totalMinutes % 60;

            return hours + "h " + Pad2(minutes) + "m";
        }

        private string FormatUtcOffset()
        {
            int totalMinutes = (utcOffsetHours * 60) + utcOffsetMinutes;
            string sign = totalMinutes >= 0 ? "+" : "-";
            if (totalMinutes < 0) totalMinutes = -totalMinutes;

            int hours = totalMinutes / 60;
            int minutes = totalMinutes % 60;

            return "UTC" + sign + Pad2(hours) + ":" + Pad2(minutes);
        }

        private string DayName(int day)
        {
            if (day == 0) return "Sunday";
            if (day == 1) return "Monday";
            if (day == 2) return "Tuesday";
            if (day == 3) return "Wednesday";
            if (day == 4) return "Thursday";
            if (day == 5) return "Friday";
            if (day == 6) return "Saturday";
            return "Unknown";
        }

        private string Pad2(int value)
        {
            if (value < 10) return "0" + value;
            return value.ToString();
        }
    }
}
