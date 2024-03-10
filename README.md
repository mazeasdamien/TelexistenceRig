# Telexistence Rig Control System

This repository contains a C# console application for controlling a Telexistence Rig, integrating a Fanuc robotic system and Microsoft Azure Kinect sensors. The communication between components is facilitated using DDS (Data Distribution Service).

## Overview

### Fanuc Robotic System Integration

- The `FanucManager` class manages the connection to the Fanuc robot, sets target joint angles, and updates the Unity scene's transformation based on the received data.
- The `FRCRobot` class from the `FRRobot` namespace is used for interacting with the Fanuc robotic system.

### Microsoft Azure Kinect Integration

- Azure Kinect sensors are utilized for capturing video and point cloud data.
- The program configures and starts the Kinect cameras based on user choices regarding video and point cloud usage.

### DDS Communication

- DDS communication is facilitated through the `Rti.Dds` namespaces.
- Different threads are used for various DDS publishers and subscribers, such as OperatorRequests, RobotState, RobotAlarm, Teleoperation (Teleop), Reachability, and Path.
- DDS Quality of Service (QoS) settings are loaded from an external XML file (`TelexistenceRig.xml`).
- The program uses dynamic data types for DDS communication, with methods like `SetupDataWriter` and `SetupDataReader` for setting up DataWriters and DataReaders.

### Configuration Menu

- The application provides a configuration menu allowing the user to select simulation mode, Kinect video usage, and Kinect point cloud usage.

### Shutdown Handling

- The program catches the CTRL-C event to exit the console cleanly, stopping Kinects and interrupting threads before disposing of DDS resources and exiting.

## Dependencies

- FRRobot library for Fanuc robotic system interaction.
- Microsoft.Azure.Kinect.Sensor library for Azure Kinect integration.
- Rti.Dds.Core library for DDS communication.

## Configuration

- DDS Quality of Service settings are defined in `TelexistenceRig.xml`.
