// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// Sample code for the "Monitor a building with Digital Twins" tutorials:

var spaceAvailFresh = "AvailableAndFresh";
var temperatureType = "Temperature";
var temperatureThreshold = 22;

function process(telemetry, executionContext) {
    try {
       // Log SensorId and Message
       log(`Sensor ID: ${telemetry.SensorId}. `);
       log(`Sensor value: ${JSON.stringify(telemetry.Message)}.`);

       // Get sensor metadata
       var sensor = getSensorMetadata(telemetry.SensorId);
       
       // Retrieve the sensor reading
       var parseReading = JSON.parse(telemetry.Message);
       
       // Set the sensor reading as the current value for the sensor.
       setSensorValue(telemetry.SensorId, sensor.DataType, parseReading.SensorValue);
       
       // Get parent space
       var parentSpace = sensor.Space();
       
       // Get children sensors from the same space
       var otherSensors = parentSpace.ChildSensors();
       
       var temperatureSensor = otherSensors.find(function(element) {
           return element.DataType === temperatureType;
       });
 
       // get latest values for above sensors
       var temperatureValue = getFloatValue(temperatureSensor.Value().Value);
       
       // Return if no motion, temperature, or carbonDioxide found
       if(temperatureValue === null){
           sendNotification(telemetry.SensorId, "Sensor", "Error: temperature is null, returning");
           return;
       }
        
       var alert = "Room with comfortable temperature is available.";
       var noAlert = "Conditions to work in this room are not good enough. Temperature is above °22C";
    
       // If sensor values are within range and room is available
       if(temperatureValue < temperatureThreshold) {
           log(`${alert}. Temperature: ${temperatureValue}.`);
           
           // log, notify and set parent space computed value
           setSpaceValue(parentSpace.Id, spaceAvailFresh, alert);
           
           // Set up notification for this alert
           parentSpace.Notify(JSON.stringify(alert));
       }
       else {
           log(`${noAlert}. Temperature: ${temperatureValue}.`);
           
           // log, notify and set parent space computed value
           setSpaceValue(parentSpace.Id, spaceAvailFresh, noAlert);
       }
   }
   catch (error)
   {
       log(`An error has occurred processing the UDF Error: ${error.name} Message ${error.message}.`);
   }
}
function getFloatValue(str) {
   if(!str) {
       return null;
   }
   return parseFloat(str);
}