// This file is part of openSMILE.
// (c) 2016 by audEERING GmbH. All rights reserved.
// See file COPYING for details on licensing terms.
//
// Written by Florian Eyben. fe@audeering.com

[componentInstances:cComponentManager]
instance[bandEnergy].type=cSimpleMessageSender

[bandEnergy:cSimpleMessageSender]
reader.dmLevel = vad_VAD_voice
sendPeriodically = 2
messageName = vad
messageType = vad
messageRecp = jniMessageInterface
enableDebugSender = 0
