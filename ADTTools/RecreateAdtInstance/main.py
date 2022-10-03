import sys
import time

from jlog import logger
from azure.mgmt.digitaltwins import AzureDigitalTwinsManagementClient
from azure.mgmt.digitaltwins.v2022_05_31.models import DigitalTwinsResource
from azure.mgmt.digitaltwins.v2022_05_31.models import DigitalTwinsEndpointResource
from azure.mgmt.digitaltwins.v2022_05_31.models import EventHub
from azure.mgmt.digitaltwins.v2022_05_31.models import EventGrid
from azure.mgmt.digitaltwins.v2022_05_31.models import ServiceBus
from azure.identity import DefaultAzureCredential
from tqdm import tqdm
from halo import Halo


if len(sys.argv) < 4:
    logger.error(
        'ADT instance name must be provided as arg[1]. SubscriptionId must be provided as arg[2] Exiting. ResourceGroup as arg[3]')
    exit()

instance_name = sys.argv[1]
subscription_id = sys.argv[2]
resource_group = sys.argv[3]
api_version = "2022-05-31"

credential_provider = DefaultAzureCredential()
# noinspection PyTypeChecker
dt_resource_client = AzureDigitalTwinsManagementClient(credential_provider, subscription_id, api_version)

instance = None

try:
    instance = dt_resource_client.digital_twins.get(resource_group, instance_name)
except:
    logger.error("DigitalTwins instance not found. Exiting.")
    exit()

logger.info("Succesfully retrieved %s.", instance.name)
# noinspection PyTypeChecker
endpoints = list(dt_resource_client.digital_twins_endpoint.list(resource_group, instance_name))


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


bar_format = "{l_bar}{bar}"
bar_color = "yellow"

delete_start = round(time.time() * 1000)
delete_instance = dt_resource_client.digital_twins.begin_delete(resource_group, instance_name)
cycles = 0
d_progress_bar = tqdm(total=10_000, desc="Deleting DigitalTwins Instance", leave=False, bar_format=bar_format, colour=bar_color)

while not delete_instance.done():
    if cycles < 9_900:
        time.sleep(0.1)
        d_progress_bar.update(25)
        cycles += 25

delete_end = round(time.time() * 1000)
d_progress_bar.update(10_000 - cycles)
d_progress_bar.close()

logger.info("Deleted DigitalTwins instance in %d milliseconds.", delete_end - delete_start)

# break_point = input()

logger.info("Creating new DigitalTwins instance %s", instance_name)
dt_resource = DigitalTwinsResource(location=instance.location, identity=instance.identity)

create_start = round(time.time() * 1000)
create_instance = dt_resource_client.digital_twins.begin_create_or_update(resource_group, instance_name, dt_resource)
cycles = 0
c_progress_bar = tqdm(total=15_000, desc="Creating DigitalTwins Instance", leave=False, bar_format=bar_format, colour=bar_color)

while not create_instance.done():
    if cycles < 14_850:
        time.sleep(0.1)
        c_progress_bar.update(25)
        cycles += 25

create_end = round(time.time() * 1000)
c_progress_bar.update(15_000 - cycles)
c_progress_bar.close()

logger.info("Created DigitalTwins instance in %d milliseconds", create_end - create_start)

logger.info("Creating Endpoints")
if len(endpoints) > 0:
    updated_endpoints = list()
    for endpoint in endpoints:
        updated_endpoint = dt_resource_client.digital_twins_endpoint.begin_create_or_update(
            resource_group_name=resource_group,
            resource_name=instance_name,
            endpoint_name=endpoint.name,
            endpoint_description=DigitalTwinsEndpointResource(properties=map_endpoint(endpoint))
        )

        updated_endpoints.append(updated_endpoint)

    num_endpoints = len(updated_endpoints)

    spinner = Halo(text='Creating Endpoints.', spinner='dots')
    spinner.start()
    while True:
        num_complete = 0

        for endpoint in updated_endpoints:
            if endpoint.done():
                num_complete += 1

        if num_complete is num_endpoints:
            break

        time.sleep(0.5)

    spinner.stop()

logger.info("Done.")
dt_resource_client.close()
