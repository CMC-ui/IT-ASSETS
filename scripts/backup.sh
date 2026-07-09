#!/bin/bash
# Nightly bash script to backup Docker volumes to a secondary location
# Usage: ./backup.sh

# Directory to backup to
BACKUP_DIR="/mnt/secondary_drive/backups/itassets_$(date +%Y%m%d_%H%M%S)"
mkdir -p "$BACKUP_DIR"

# Docker volume locations on Rocky Linux (typically /var/lib/docker/volumes/)
# The volume names assume the docker-compose project is named "it-assets"
APP_DB_VOLUME="/var/lib/docker/volumes/it-assets_app_data/_data"
UPLOADS_VOLUME="/var/lib/docker/volumes/it-assets_uploads_data/_data"

echo "Starting backup of IT Assets volumes..."

if [ -d "$APP_DB_VOLUME" ]; then
    cp -r "$APP_DB_VOLUME" "$BACKUP_DIR/app_data"
    echo "App DB volume backed up."
else
    echo "App DB volume not found!"
fi

if [ -d "$UPLOADS_VOLUME" ]; then
    cp -r "$UPLOADS_VOLUME" "$BACKUP_DIR/uploads_data"
    echo "Uploads volume backed up."
else
    echo "Uploads volume not found!"
fi

echo "Backup completed to $BACKUP_DIR"
