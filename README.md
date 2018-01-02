# Panda
An developer oriented alt + space launcher inspired by Launchy and Hain. Written with extensibility in mind.
The goal is to have a framework for easily adding new functionality and extending existing functionality.

The motivation comes from wanting more from existing convenience applications and not liking their extension models.

### Why "Panda"
1. Names are hard
1. Pandas are lazy and laziness drove me to write this
1. I figured a panda logo would be pretty easy to make

### Design Notes
This application makes heavy use of MEF. The philosophy behind the application architecture is that as a
application programmer you can just "ask" for things to be available for you when its your turn to do something.
Conversely, if you are framework designer, you can simply export an implentation for a specific interface and then
whoever has a reference to that interface can "ask" for your service. Moreover, you should be able to do all this
without configuration. The goal is to be able to drag + drop a new .dll and on application restart -have new functionality.

For example, you might want to create an alerting launcher that periodically checks splunk or logic monitor
for signs of well known problems. Upon triggering an alert, a toast notification pops up, and upon clicking the 
toast notification the UI renders showing you the details of the altert.

Also, you know, games and stuff.

### Concepts
###### Launchers
Launchers are `Window`s that provide some kind of user interface that does something useful. If you want to extend 
Panda to do something cool, you probably want to start here. Pretty much anything you can you do with a `Window`
you can make into a launcher.

Recommendations:
1. Favor the MVVM design pattern
1. Use the provided launchers as exemplars


###### Services
Services are whatever you want them to be. I make no restrictions on the API you can provide. That being said,
I recommend the following guide lines:

1. Expose `IObservable`s for changes in state
1. Favor asynchronous APIs to synchronous
1. TBD