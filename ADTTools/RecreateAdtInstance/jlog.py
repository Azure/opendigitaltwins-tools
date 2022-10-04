import logging

from tqdm import tqdm
from halo import Halo


class StatusBar:
    @staticmethod
    def init(total, msg):
        return tqdm(total=total,
                    desc=msg,
                    leave=False,
                    bar_format="{l_bar}{bar}",
                    colour="yellow")


class Spinner:
    @staticmethod
    def init(msg):
        return Halo(text=msg, spinner="dots", placement="right")


class LogFormat(logging.Formatter):
    grey = "\x1b[38;20m"
    green = "\x1b[32;20m"
    yellow = "\x1b[33;20m"
    red = "\x1b[31;20m"
    bold_red = "\x1b[31;1m"
    reset = "\x1b[0m"
    format = "%(levelname)s - %(asctime)s - %(name)s: %(message)s"

    FORMATS = {
        logging.DEBUG:    green + format + reset,
        logging.INFO:     green + format + reset,
        logging.WARNING:  yellow + format + reset,
        logging.ERROR:    red + format + reset,
        logging.CRITICAL: bold_red + format + reset
    }

    def format(self, record):
        log_fmt = self.FORMATS.get(record.levelno)
        formatter = logging.Formatter(log_fmt)
        return formatter.format(record)


logger = logging.getLogger("RecreateAdtInstance")
logger.setLevel(logging.DEBUG)

console_handler = logging.StreamHandler()
console_handler.setLevel(logging.DEBUG)
console_handler.setFormatter(LogFormat())

logger.addHandler(console_handler)
