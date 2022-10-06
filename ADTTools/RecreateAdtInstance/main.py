import sys
import time

from jlog import logger
from azure.mgmt.digitaltwins import AzureDigitalTwinsManagementClient
from azure.mgmt.digitaltwins.v2022_05_31.models import DigitalTwinsResource
from azure.identity import DefaultAzureCredential
from tqdm import tqdm

if len(sys.argv) < 4:
    logger.error(
        'ADT instance name must be provided as arg[1]. SubscriptionId must be provided as arg[2] Exiting. ResourceGroup as arg[3]')
    exit()

instance_name = sys.argv[1]
subscription_id = sys.argv[2]
resource_group = sys.argv[3]
api_verison = "2022-05-31"

credential_provider = DefaultAzureCredential()
digitalTwinsClient = AzureDigitalTwinsManagementClient(credential_provider, subscription_id, api_verison)

instance = None

try:
    instance = digitalTwinsClient.digital_twins.get(resource_group, instance_name)
except:
    logger.error("DigitalTwins instance not found. Exiting.")
    exit()

logger.info("Succesfully retrieved %s.", instance.name)

bar_format = "{l_bar}{bar}"
bar_color = "yellow"

delete_start = round(time.time() * 1000)
delete_instance = digitalTwinsClient.digital_twins.begin_delete(resource_group, instance_name)
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
dt_resource = DigitalTwinsResource(location=instance.location)

create_start = round(time.time() * 1000)
create_instance = digitalTwinsClient.digital_twins.begin_create_or_update(resource_group, instance_name, dt_resource)
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

logger.info("Done.")
digitalTwinsClient.close()
