# LearningHub
## Initialize the connector
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
