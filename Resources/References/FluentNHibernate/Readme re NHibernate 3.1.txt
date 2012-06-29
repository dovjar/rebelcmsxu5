A note about NHibernate.XmlSerializers.dll:

Deserializing NH's Configuration from a file is a common way of improving the speed of app startup.
However, NH internally uses an XMlDeserializer for its HbmMappings so even if you use a BinaryFormatter, it's still very slow because XmlSerializer has to emit a dynamic assembly for deserialization.
I've created this assembly ahead of time using the commandline:

sgen.exe /a:NHibernate.dll /t:NHibernate.Cfg.MappingSchema.HbmMapping /compiler:/keyfile:NHibernate.snk

The deserialization of NH Config (at App_Data/HiveConfig/Configuration*.bin) has now gone from 780ms at app startup to 181ms (relative speeds - ymmv)

Alex N May 2012