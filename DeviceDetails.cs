using System.ComponentModel.DataAnnotations;

namespace Geotab.CustomerOnboardngStarterKit
{
    /// <summary>
    ///  Object containing information about a <see cref="Device"/>.
    /// </summary>
    class DeviceDetails
    {
        /// <summary>
        /// The device serial number.
        /// </summary>
        /// <value></value>
        [Display(Name = "SerialNumber", Order = 1)]
        public string SerialNumber { get; set; }

        /// <summary>
        /// A description to be used as the vehicle name.
        /// </summary>
        /// <value></value>
        [Display(Name = "Name", Order = 2)]
        public string Name { get; set; }

        /// <summary>
        /// Master toggle to enable the device buzzer. When set to <c>false</c>, the device will not provide driver feedback of any kind.
        /// </summary>
        /// <value></value>
        [Display(Name = "EnableDeviceBeeping", Order = 3)]
        public bool EnableDeviceBeeping { get; set; }

        /// <summary>
        /// A value mainly used for enable or disable driver identification reminder. If it is used in conjunction with vehicle relay circuits, it can force the driver to swipe the driver key before starting the vehicle.
        /// </summary>
        /// <value></value>
        [Display(Name = "EnableDriverIdentificationReminder", Order = 4)]
        public bool EnableDriverIdentificationReminder { get; set; }

        /// <summary>
        /// With enableDriverIdentificationReminder being true, it is used to define the delay before the driver identification reminder is sent out if the driver key has not been not swiped. The maximum value of this property is 255. When it is less or equal to 180, it indicates the number of seconds of the delay. When it is greater than 180, the delay increases 30 seconds for every increment of one of this property. For example, 180 indicates 180 seconds, 181 indicates 210 seconds, and 182 indicates 240 seconds. Maximum [255]
        /// </summary>
        /// <value></value>
        [Display(Name = "DriverIdentificationReminderImmobilizeSeconds", Order = 5)]
        public int DriverIdentificationReminderImmobilizeSeconds { get; set; }

        /// <summary>
        /// Toggle to enable beeping when the vehicle's RPM exceeds the 'engineRpmBeepValue'.
        /// </summary>
        /// <value></value>
        [Display(Name = "EnableBeepOnEngineRpm", Order = 6)]
        public bool EnableBeepOnEngineRpm { get; set; }

        /// <summary>
        /// The RPM value that when exceeded triggers device beeping.
        /// </summary>
        /// <value></value>
        [Display(Name = "EngineRpmBeepValue", Order = 7)]
        public int EngineRpmBeepValue { get; set; }

        /// <summary>
        /// Toggle to enable beeping when the vehicle idles for more than idleMinutesBeepValue.
        /// </summary>
        /// <value></value>
        [Display(Name = "EnableBeepOnIdle", Order = 8)]
        public bool EnableBeepOnIdle { get; set; }

        /// <summary>
        /// The number of minutes of allowed idling before device beeping starts. enableBeepOnIdle must be enabled.
        /// </summary>
        /// <value></value>
        [Display(Name = "IdleMinutesBeepValue", Order = 9)]
        public int IdleMinutesBeepValue { get; set; }

        /// <summary>
        /// A toggle to beep constantly when the vehicle reaches the speed set in 'speedingStartBeepingSpeed', and do not stop until the vehicle slows below the 'speedingStopBeepingSpeed' speed. To only beep briefly (instead of continuously), enable 'enableBeepBrieflyWhenApprocahingWarningSpeed'.
        /// </summary>
        /// <value></value>
        [Display(Name = "EnableBeepOnSpeeding", Order = 10)]
        public bool EnableBeepOnSpeeding { get; set; }

        /// <summary>
        /// The speeding on value in km/h. When 'enableBeepOnSpeeding' is enabled, the device will start beeping when the vehicle exceeds this speed.
        /// </summary>
        /// <value></value>
        [Display(Name = "SpeedingStartBeepingSpeed", Order = 11)]
        public int SpeedingStartBeepingSpeed { get; set; }

        /// <summary>
        /// The speeding off value in km/h. When 'enableBeepOnSpeeding' is enabled, once beeping starts, the vehicle must slow down to this speed for the beeping to stop.
        /// </summary>
        /// <value></value>
        [Display(Name = "SpeedingStopBeepingSpeed", Order = 12)]
        public int SpeedingStopBeepingSpeed { get; set; }

        /// <summary>
        /// Toggle to enable speed warning value for the vehicle. When enabled [true], only beep briefly (instead of continuously), when 'speedingStopBeepingSpeed' value is exceeded. 'speedingStartBeepingSpeed' must also be enabled.
        /// </summary>
        /// <value></value>
        [Display(Name = "EnableBeepBrieflyWhenApprocahingWarningSpeed", Order = 13)]
        public bool EnableBeepBrieflyWhenApprocahingWarningSpeed { get; set; }

        /// <summary>
        /// Toggle to enable beeping when any of the acceleration thresholds are exceeded by device accelerometer readings.
        /// </summary>
        /// <value></value>
        [Display(Name = "EnableBeepOnDangerousDriving", Order = 14)]
        public bool EnableBeepOnDangerousDriving { get; set; }

        /// <summary>
        /// The acceleration warning accelerometer threshold (y axis) value for the vehicle. A positive value that when exceeded will trigger device beeping. Threshold value to mS2 conversion (threshold * 18 = milli-g / 1000 = g / 1.0197162 = mS2).
        /// </summary>
        /// <value></value>
        [Display(Name = "AccelerationWarningThreshold", Order = 15)]
        public int AccelerationWarningThreshold { get; set; }

        /// <summary>
        /// The braking warning accelerometer threshold (y axis) value for the vehicle. A negative value that when exceeded will trigger device beeping. Threshold value to mS2 conversion (threshold * 18 = milli-g / 1000 = g / 1.0197162 = mS2).
        /// </summary>
        /// <value></value>
        [Display(Name = "BrakingWarningThreshold", Order = 16)]
        public int BrakingWarningThreshold { get; set; }

        /// <summary>
        /// The cornering warning threshold (x axis) value for the vehicle. A positive value that when exceeded will trigger device beeping (the additive inverse is automatically applied: 26/-26). Threshold value to mS2 conversion (threshold * 18 = milli-g / 1000 = g / 1.0197162 = mS2).
        /// </summary>
        /// <value></value>
        [Display(Name = "CorneringWarningThreshold", Order = 17)]
        public int CorneringWarningThreshold { get; set; }

        /// <summary>
        /// Value which toggles beeping if an unbuckled seat belt is detected. This will only work if the device is able to obtain seat belt information from the vehicle.
        /// </summary>
        /// <value></value>
        [Display(Name = "EnableBeepWhenSeatbeltNotUsed", Order = 18)]
        public bool EnableBeepWhenSeatbeltNotUsed { get; set; }

        /// <summary>
        /// The value in km/h that below will not trigger 'enableBeepWhenSeatbeltNotUsed'.
        /// </summary>
        /// <value></value>
        [Display(Name = "SeatbeltNotUsedWarningSpeed", Order = 19)]
        public int SeatbeltNotUsedWarningSpeed { get; set; }

        /// <summary>
        /// Value which toggles monitoring both passenger and driver unbuckled seat belt, otherwise only the driver is monitored.
        /// </summary>
        /// <value></value>
        [Display(Name = "EnableBeepWhenPassengerSeatbeltNotUsed", Order = 20)]
        public bool EnableBeepWhenPassengerSeatbeltNotUsed { get; set; }

        /// <summary>
        /// Value which toggles device beeping when the vehicle is reversing.
        /// </summary>
        /// <value></value>
        [Display(Name = "BeepWhenReversing", Order = 21)]
        public bool BeepWhenReversing { get; set; }
   }
}
