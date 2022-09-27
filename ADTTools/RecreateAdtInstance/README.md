# Recreate ADT Instance

This project has been created to quickly delete and recreate ADT instance with the same instance name. This allows instances to be recreated without having the use a different host endpoint for the ADT instance.

## Requirements
- Currently functionality has been verified only with Python 3.1 or higher. 
- On Windows, verify that the System PATH variable contains only one Python version. By going to `System Properties -> Advanced -> Environment`. If installing Python for the first time, a system restart may be required. 

## Usage
Run
    ```shell
    pip install -r requirements.txt
    ```
To install dependencies or use `make`

Example
    ```shell
    python main.py <INSTANCE_NAME> <SUBSCRIPTION_ID> <RESOURCE_GROUP>
    ```
The DigitalTwins instance name, subscription id, and resource groups are required arguments for use.
