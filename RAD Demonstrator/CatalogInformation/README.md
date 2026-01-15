# RAD Demonstrator

This package is designed to help users get started with Relational Anomaly Detection (RAD).
It includes all the components needed to follow the "Working with Relational Anomaly Detection" tutorial,
available in the DataMiner documentation.
You can find the tutorial at docs.dataminer.services by navigating to DATAMINER TUTORIALS > DATAMINER ANALYTICS > Working With Relational Anomaly Detection. The package is also featured in the 2025 Empower replay video on Relational Anomaly Detection.

The package consists of 6 elements that represent DAB transmitters (with dummy data). After deployment,
the elements
can be found in Cube under the DataMiner Catalog > Using Relational Anomaly Detection > London view and are named:
- RAD - Commtia LON 1
- RAD - Commtia LON 2
- RAD - Commtia LON 3
- RAD - Commtia LON 4
- RAD - Commtia LON 5
- RAD - Commtia LON 6

The package also contains 2 automation scripts that you can use as a basis for more advanced RAD configurations using the RAD API. The scripts are: 

- Add Single Groups: create a RAD Model Group for every DAB Transmitter.
- Add Shared Model Group: create one Shared Model Group to model all DAB Transmitters together.

The scripts can be explored through the Automation app, under the folder DataMiner Catalog > RAD Demonstrator.