# Recreate Digital Twins Instance

This project has been created to quickly delete and recreate ADT instance with the same instance name. This allows 
instances to be recreated without having the use a different host endpoint for the ADT instance. Additionally, this 
will also recreate endpoints.

- **Note**: Endpoint Retention is based on the assumption that endpoints have originally been created using **identity**
based authentication.

## Requirements
- Currently, functionality has been verified only with Python **3.10** or higher. 
- On Windows, verify that the System PATH variable contains only one Python version. By going to `System Properties -> Advanced -> Environment`. If installing Python for the first time, a system restart may be required.

## Usage
### Install Dependencies
Run:

```shell
pip install -r requirements.txt
```
This will install required dependencies for this script. However, you can also simply just run `make` if you have **make**
installed on your machine.

### Run Script
Run:

```shell
python main.py <INSTANCE_NAME> <SUBSCRIPTION_ID> <RESOURCE_GROUP>
```

The DigitalTwins instance name, subscription id, and resource groups are required arguments for use.
- **Note**: The instance name refers to the name of the ADT instance, not the host name.