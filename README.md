# LearningHub



The objective of the LearningHub (LH) is to collect data from different sources and generate a unified multimodal digital experience of a learning task. The LH has two main functions that allow it to contribute to the study and enhancement of the learning process. First it creates multimodal recordings of learning tasks. Second the LH retrieves critical data from the different sources, and forwards this data to applications designed to provide immediate feedback to learners.
 
## Multimodal Recordings of a learning task

It is neither feasible nor desirable to have applications retrieving, recording and analyzing data from learners all the time. The LH works by recording only specific learning tasks, this means that the user of the LH needs to manually start and stop a recording.
Communication between LH and data providers
To start, stop and generate a unified multimodal recording the LH needs to communicate with the providers of data. The first step into creating a recording is to define the communication channels between the data providers and the LH. For each source provider one needs to define:
·         Name of the provider.
·         Path where the provider will be executed (file path in case the provider will run on the same computer or IP address in case the provider will run remotely)
·         Remote or local execution of the provider
·         Port number for the TCP listener socket that will receive instructions from the provider.
·         Port number of the TCP sender socket that will send instructions to the provider.
·         Port number for the TCP socket that will receive files from the provider.
·         Port number for the UDP socket that will receive critical real-time data from the provider.
·         Port number for the UDP socket that will send streams of data to the provider.
·         Boolean value stating if the provider will be used for the specific recording.
 
 
 
From the side of the providers the communication can be handled through the connectorHub dynamic library that is included with the LH solution, or by manually programing the socket communication. Currently LH solution contains a dynamic Library that works with .Net projects and a dynamic library that works with windows universal platform.
In order to be able to communicate between each other the LH and each provider must agree on the same communication channels. This is handled with a configuration file named portConfig.txt. Each of the first five lies of this file contains the port numbers that were previously defined. Finally the sixth line of the file contains the IP address of the LH. In the case of providers running in the same computer as the LH, the LH automatically creates this configuration file in the right location. In the case of providers running in other computers this configuration file needs be created manually.
After configuring the communication channels one need to run the provider applications. Provider applications running on the same computer as the LH will be started automatically. At this point the LH will internally create a handle for each one of the providers in order to communicate with them. Once a provider is ready to start capturing data, it will send an IamReady signal to the LH. At this point through the LH it is possible to start and stop a Multimodal recording of a learning task.

##Example on how to use the ConnectorHub Library:


Add the following lines to initialize the connector: 
```
myConectorHub = new ConnectorHub.ConnectorHub();
                myConectorHub.init();
                myConectorHub.sendReady();
                myConectorHub.startRecordingEvent += MyConectorHub_startRecordingEvent;
                myConectorHub.stopRecordingEvent += MyConectorHub_stopRecordingEvent;
                setValuesNames();
```


Then you need to add a list with the attribute names: 
```
myConectorHub.setValuesName(names);
```
E.g. ['AnkleLeftX','AnkleLeftY',...]

## Data Storing.
As discussed previously each data provider retrieves different type of data and at a different rate. In order to fuse the data coming from different providers in one unified multimodal recording we used the following Recording format:
A multimodal recording is composed by a collection of RecordingObjects. Each provider generates one RecordingObject. A RecordingObject is composed by a recordingId, an applicationName (name of the data provider) and a collection of FrameObjects. Each FrameObject consist of a frameStamp (time passed since the beginning of the recording) and a dictionary containing the name of the attributes stored for each frame and the current values of these attributes.
Once the recording stops all RecordingObjects are collected by the MLH.
 
 
 
## Multimodal Data Synchronization.
The first instruction executed by data providers after receiving a StartRecording instruction is to take note of their current time, which is stored as the starting time of the recording. During the recording once a data provider has a frame ready to be stored, it checks for the current time and subtracts from it the starting time. By assuming the clocks from the data providers run at the same speed, and that all data providers received the StartRecording instruction almost at the same time, this strategy allows a good enough synchronization of multimodal data.


 ##   Immediate Feedback


To keep things simple from the side of the tutor it is recommended to only transmit critical data from the providers to the LH. An example of critical data could be the instruction to “Speak Louder” in case a microphone application detects that the learner is speaking too soft during the specific learning task. To keep things as simple as possible this type of instruction is transmitted from the provider to the LH via UDP sockets.
The MLH can then forward the received instructions to applications design to provide feedback to learners. This applications can be ambient displays, augmented reality glasses, etc. Establishing the communication between the LH and the immediate feedback applications is a very similar process to the one for establishing the link between the LH and providers. Before starting a recording one needs to manually select the feedback applications that will be used and define the communication channels. For each feedback application one needs to define:
·	The Name of the application
·	Path to reach the application (IP address)
·	Port number for sending operational instructions via a TCP socket.
·	Port number for streaming feedback instructions via a UDP socket.
 
The feedback applications need to open the socket communications in order to receive the information that comes from the LH. This can be done through the use of the dynamic Libraries included in the LH solution (the currently supported libraries are for the .Net framework and Windows Universal Platform), or by manually programing the TCP and UDP sockets.



