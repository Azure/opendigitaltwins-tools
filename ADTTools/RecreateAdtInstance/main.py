import sys
import time

from jlog import logger
from jlog import ProgressBar
from jlog import Spinner
from azure.mgmt.digitaltwins import AzureDigitalTwinsManagementClient
from azure.core.exceptions import HttpResponseError
from azure.mgmt.digitaltwins.v2022_05_31.models import DigitalTwinsResource
from azure.mgmt.digitaltwins.v2022_05_31.models import DigitalTwinsEndpointResource
from azure.mgmt.digitaltwins.v2022_05_31.models import EventHub
from azure.mgmt.digitaltwins.v2022_05_31.models import EventGrid
from azure.mgmt.digitaltwins.v2022_05_31.models import ServiceBus
from azure.identity import DefaultAzureCredential


def map_endpoint(resource):
    props = resource.properties

    match props.endpoint_type:
        case "EventHub":
            return EventHub(
                authentication_type=props.authentication_type,
                endpoint_uri=props.endpoint_uri,
                entity_path=props.entity_path
            )
        case "EventGrid":
            return EventGrid(
                topic_endpoint=props.topic_endpoint,
                access_key1=props.access_key1,
                authentication_type=props.authentication_type
            )
        case "ServiceBus":
            return ServiceBus(
                authentication_type=props.authentication_type,
                endpoint_uri=props.endpoint_uri,
                entity_path=props.entity_path
            )


def delete_digital_twins_instance():
    delete_start = round(time.time() * 1000)
    delete_instance = dt_resource_client.digital_twins.begin_delete(resource_group, instance_name)
    cycles = 0
    progress_bar = ProgressBar.new(total=10_000, msg="Deleting DigitalTwins Instance")

    while not delete_instance.done():
        if cycles < 9_900:
            time.sleep(0.1)
            progress_bar.update(25)
            cycles += 25

    delete_end = round(time.time() * 1000)
    progress_bar.update(10_000 - cycles)
    progress_bar.close()

    return delete_end - delete_start


def create_digital_twins_instance():
    logger.info("Creating new DigitalTwins instance %s", instance_name)
    dt_resource = DigitalTwinsResource(location=instance.location, identity=instance.identity)

    create_start = round(time.time() * 1000)
    create_instance = dt_resource_client.digital_twins.begin_create_or_update(resource_group, instance_name, dt_resource)
    cycles = 0
    progress_bar = ProgressBar.new(total=15_000, msg="Creating DigitalTwins Instance")

    while not create_instance.done():
        if cycles < 14_850:
            time.sleep(0.1)
            progress_bar.update(25)
            cycles += 25

    create_end = round(time.time() * 1000)
    progress_bar.update(15_000 - cycles)
    progress_bar.close()

    return create_end - create_start


def create_endpoints():
    num_endpoints = len(endpoints)
    logger.info("Recreating %d endpoint(s).", num_endpoints)
    start_time = round(time.time() * 1000)
    updated_endpoints = list()

    for endpoint in endpoints:
        updated_endpoint = dt_resource_client.digital_twins_endpoint.begin_create_or_update(
            resource_group_name=resource_group,
            resource_name=instance_name,
            endpoint_name=endpoint.name,
            endpoint_description=DigitalTwinsEndpointResource(properties=map_endpoint(endpoint))
        )

        updated_endpoints.append(updated_endpoint)

    spinner = Spinner.new("Creating Endpoints.")
    spinner.start()

    num_complete = 0
    while True:

        for index in range(0, num_endpoints):
            current_request = updated_endpoints[index]

            if current_request is None:
                continue

            if current_request.done():
                updated_endpoints[index] = None
                num_complete += 1
                spinner.text = f"Creating Endpoints. {num_complete}/{num_endpoints}"

        if num_complete is num_endpoints:
            break

        time.sleep(0.1)

    end_time = round(time.time() * 1000)
    spinner.stop()
    return end_time - start_time


if len(sys.argv) < 4:
    logger.error(
        'ADT instance name must be provided as arg[1]. SubscriptionId must be provided as arg[2] Exiting. ResourceGroup as arg[3]')
    exit()

instance_name   = sys.argv[1]
subscription_id = sys.argv[2]
resource_group  = sys.argv[3]

api_version         = "2022-05-31"
credential_provider = DefaultAzureCredential()
dt_resource_client  = AzureDigitalTwinsManagementClient(credential_provider, subscription_id, api_version)

instance = None

try:
    instance = dt_resource_client.digital_twins.get(resource_group, instance_name)
except HttpResponseError as e:
    logger.exception(e)
    exit()

logger.info("Successfully retrieved %s.", instance.name)

endpoints = list(dt_resource_client.digital_twins_endpoint.list(resource_group, instance_name))

logger.info("Deleted DigitalTwins instance in %d milliseconds.", delete_digital_twins_instance())

logger.info("Created DigitalTwins instance in %d milliseconds", create_digital_twins_instance())

if len(endpoints) > 0:
    logger.info("Created endpoints in %d milliseconds", create_endpoints())

logger.info("Done.")
dt_resource_client.close()
