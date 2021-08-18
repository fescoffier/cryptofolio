import datetime
import logging
import json
import subprocess

logger = logger = logging.getLogger()
logger.setLevel(logging.DEBUG)
logger.addHandler(logging.StreamHandler())
date_format = "%Y-%m-%dT%H:%M:%SZ"
min_keep = 3
repositories = [
    "api",
    "app",
    "jobs/collector",
    "jobs/handlers"
]


def get_deletable_tags_args(repository: str):
    logger.info("Retreiving tags for the %s repository.", repository)
    result = subprocess.run(f"doctl registry repository list-tags {repository} --output json",
                            shell=True,
                            capture_output=True)
    if result.returncode != 0:
        return None
    logger.debug("Found tags:\n%s", result.stdout.decode("utf-8"))
    tags = json.loads(result.stdout)
    tags.sort(key=lambda t: datetime.datetime.strptime(t["updated_at"], "%Y-%m-%dT%H:%M:%SZ"), reverse=True)
    tags = tags[min_keep:]
    logger.debug("Deletable tags: %s", tags)
    if len(tags) > 0:
        return " ".join([tag["tag"] for tag in tags])
    else:
        return None

def delete_tags(repository: str, tags: str):
    result = subprocess.run(f"doctl registry repository delete-tag {repository} {tags} -f",
                            shell=True,
                            capture_output=True)
    if result.returncode == 0:
        logger.info("Following tags were deleted: %s", tags)

if __name__ == "__main__":
    for repository in repositories:
        deletable_tags = get_deletable_tags_args(repository)
        if deletable_tags != None:
            delete_tags(repository, deletable_tags)
