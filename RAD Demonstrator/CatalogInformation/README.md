# RAD Demonstrator

This package is designed to help users get started with Relational Anomaly Detection (RAD).
It includes all the components needed to follow the "Working with Relational Anomaly Detection" tutorial,
available in the DataMiner documentation.

You can find the tutorial at docs.dataminer.services by navigating to DATAMINER TUTORIALS > DATAMINER ANALYTICS > Working With Relational Anomaly Detection. The package is also featured in the 2025 Empower replay video on Relational Anomaly Detection.

The package contains two Automation scripts:

   - Add Shared Model Group
   - Add Single Groups

You can find them in the Automation module: the scripts are stored in the *DataMiner Catalog > RAD Demonstrator* folder. The scripts
serve as a starting point if you want to create your own RAD groups using the RAD API.

The package will also create the following DataMiner elements in the Cube Surveyor under *DataMiner Catalog* > *Using Relational Anomaly Detection* > *London*:

   - RAD - Commtia LON 1
   - RAD - Commtia LON 2
   - RAD - Commtia LON 3
   - RAD - Commtia LON 4
   - RAD - Commtia LON 5

Finally, it will create 29 additional elements *Fleet-Outlier-Detection-Commtia 01*, *Fleet-Outlier-Detection-Commtia 02*,... 
until *Fleet-Outlier-Detection-Commtia 29* in the Cube Surveyor under *DataMiner Catalog* > *Using Relational Anomaly Detection* > *RAD Fleet Outlier*.

