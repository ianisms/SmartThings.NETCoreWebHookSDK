# Device Models

The SDK has stringly typed models for several devices which can be instantiated from the responses coming from the SmartThings API.  These devices also have commands that can be executed on them for example locking a lock and switching on a light switch.  The models can be found in ```ianisms.SmartThings.NETCoreWebHookSDK.Models.SmartThings```.

## AccelerationSensor

See [Acceleration Sensor](https://smartthings.developer.samsung.com/docs/api-ref/capabilities.html#Acceleration-Sensor)

### AccelerationSensorFromDynamic

Use ```AccelerationSensorFromDynamic``` to parse a AccelerationSensor from responses obtained from ```ISmartThingsAPIHelper```.

```csharp
public static AccelerationSensor AccelerationSensorFromDynamic(dynamic val,
    dynamic status = null)
```

```val``` is the value obtained from ```ISmartThingsAPIHelper.GetDeviceDetailsAsync```.

```status``` is the value obtained from ```ISmartThingsAPIHelper.GetDeviceStatusAsync```.

## AirQualitySensor

See [Air Quality Sensor](https://smartthings.developer.samsung.com/docs/api-ref/capabilities.html#Air-Quality-Sensor)

### AirQualitySensorFromDynamic

Use ```AirQualitySensorFromDynamic``` to parse a AirQualitySensor from responses obtained from ```ISmartThingsAPIHelper```.

```csharp
public static AirQualitySensor AirQualitySensorFromDynamic(dynamic val,
    dynamic status = null)
```

```val``` is the value obtained from ```ISmartThingsAPIHelper.GetDeviceDetailsAsync```.

```status``` is the value obtained from ```ISmartThingsAPIHelper.GetDeviceStatusAsync```.

## CarbonMonoxideDetector

See [Carbon Monoxide Detector](https://smartthings.developer.samsung.com/docs/api-ref/capabilities.html#Carbon-Monoxide-Detector)

### CarbonMonoxideDetectorFromDynamic

Use ```CarbonMonoxideDetectorFromDynamic``` to parse a CarbonMonoxideDetector from responses obtained from ```ISmartThingsAPIHelper```.

```csharp
public static CarbonMonoxideDetector CarbonMonoxideDetectorFromDynamic(dynamic val,
    dynamic status = null)
```

```val``` is the value obtained from ```ISmartThingsAPIHelper.GetDeviceDetailsAsync```.

```status``` is the value obtained from ```ISmartThingsAPIHelper.GetDeviceStatusAsync```.

## ContactSensor

See [Contact Sensor](https://smartthings.developer.samsung.com/docs/api-ref/capabilities.html#Contact-Sensor)

### ContactSensorFromDynamic

Use ```ContactSensorFromDynamic``` to parse a ContactSensor from responses obtained from ```ISmartThingsAPIHelper```.

```csharp
public static ContactSensor ContactSensorFromDynamic(dynamic val,
    dynamic status = null)
```

```val``` is the value obtained from ```ISmartThingsAPIHelper.GetDeviceDetailsAsync```.

```status``` is the value obtained from ```ISmartThingsAPIHelper.GetDeviceStatusAsync```.

## DoorLock

See [Lock](https://smartthings.developer.samsung.com/docs/api-ref/capabilities.html#Lock)

Named DoorLock instead of Lock due to the fact that ```Lock``` is a .NET statement.

### DoorLockFromDynamic

Use ```DoorLockFromDynamic``` to parse a DoorLock from responses obtained from ```ISmartThingsAPIHelper```.

```csharp
public static DoorLock DoorLockFromDynamic(dynamic val,
    dynamic status = null)
```

```val``` is the value obtained from ```ISmartThingsAPIHelper.GetDeviceDetailsAsync```.

```status``` is the value obtained from ```ISmartThingsAPIHelper.GetDeviceStatusAsync```.

### DoorLock Commands

lock/unlock via ```GetDeviceCommand(bool value)```, pass ```true``` to lock and ```false``` to unlock.

## LightSwitch

See [Switch](https://smartthings.developer.samsung.com/docs/api-ref/capabilities.html#Switch)

Named LightSwitch instead of Swicth due to the fact that ```Switch``` is a .NET statement.

### LightSwitchFromDynamic

Use ```LightSwitchFromDynamic``` to parse a LightSwitch from responses obtained from ```ISmartThingsAPIHelper```.

```csharp
public static LightSwitch LightSwitchFromDynamic(dynamic val,
    dynamic status = null)
```

```val``` is the value obtained from ```ISmartThingsAPIHelper.GetDeviceDetailsAsync```.

```status``` is the value obtained from ```ISmartThingsAPIHelper.GetDeviceStatusAsync```.

### LightSwitch Commands

on/off via ```GetDeviceCommand(bool value)```, pass ```true``` for on and ```false``` for off.

## MotionSensor

See [Motion Sensor](https://smartthings.developer.samsung.com/docs/api-ref/capabilities.html#Motion-Sensor)

### MotionSensorFromDynamic

Use ```MotionSensorFromDynamic``` to parse a MotionSensor from responses obtained from ```ISmartThingsAPIHelper```.

```csharp
public static MotionSensor MotionSensorFromDynamic(dynamic val,
    dynamic status = null)
```

```val``` is the value obtained from ```ISmartThingsAPIHelper.GetDeviceDetailsAsync```.

```status``` is the value obtained from ```ISmartThingsAPIHelper.GetDeviceStatusAsync```.

## PresenceSensor

See [Presence Sensor](https://smartthings.developer.samsung.com/docs/api-ref/capabilities.html#Presence-Sensor)

### PresenceSensorFromDynamic

Use ```PresenceSensorFromDynamic``` to parse a PresenceSensor from responses obtained from ```ISmartThingsAPIHelper```.

```csharp
public static PresenceSensor PresenceSensorFromDynamic(dynamic val,
    dynamic status = null,
    string presenceSensorNamePattern = null)
```

```val``` is the value obtained from ```ISmartThingsAPIHelper.GetDeviceDetailsAsync```.

```status``` is the value obtained from ```ISmartThingsAPIHelper.GetDeviceStatusAsync```.

Passing the optional ```presenceSensorNamePattern``` parameter will set the ```FriendlyName``` value to the laebl from 0 to the index of the ```presenceSensorNamePattern``` if found.

## SpeechDevice

Not yet listed in SmartThings capabilities refererence, uses the speechSynthesis capability.

### SpeechDeviceFromDynamic

Use ```SpeechDeviceFromDynamic``` to parse a SpeechDevice from responses obtained from ```ISmartThingsAPIHelper```.

```csharp
public static SpeechDevice SpeechDeviceFromDynamic(dynamic val,
    dynamic status = null)
```

```val``` is the value obtained from ```ISmartThingsAPIHelper.GetDeviceDetailsAsync```.

```status``` is the value obtained from ```ISmartThingsAPIHelper.GetDeviceStatusAsync```.

## WaterSensor

See [Water Sensor](https://smartthings.developer.samsung.com/docs/api-ref/capabilities.html#Water-Sensor)

### WaterSensorFromDynamic

Use ```WaterSensorFromDynamic``` to parse a WaterSensor from responses obtained from ```ISmartThingsAPIHelper```.

```csharp
public static WaterSensor WaterSensorFromDynamic(dynamic val,
    dynamic status = null)
```

```val``` is the value obtained from ```ISmartThingsAPIHelper.GetDeviceDetailsAsync```.

```status``` is the value obtained from ```ISmartThingsAPIHelper.GetDeviceStatusAsync```.
