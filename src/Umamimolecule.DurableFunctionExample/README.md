# Umamimolecule.DurableFunctionExample

An example Durable Function to show how to integrate the state machine into your orchestrator.

This example simulates a traffic light, with a super-simple state machine like so:

[Initializing] -> [Green] -> [Orange] -> [Red]

On entry to the green, orange and red states, an Actvity function is called which logs the state name and pauses for 5 seconds, then triggers the next state transition.

It will perform one cycle and then stop.
