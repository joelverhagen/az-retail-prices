#!/bin/bash

SCRIPT_DIR="$(dirname "$0")"
DB_URL="https://github.com/joelverhagen/data-az-retail-prices/releases/download/latest/azure-prices.db"
DEFAULT_OUT_DIR="$SCRIPT_DIR/public/data"

# Use the maximum page size to reduce round trips
PAGE_SIZE="65536"

OUT_DIR="${1:-$DEFAULT_OUT_DIR}"
DB_PATH="$OUT_DIR/db.sqlite3"

# Set up the data directory
rm -rf "$OUT_DIR"
mkdir "$OUT_DIR" --parents

# Download the latest DB
wget "$DB_URL" -O "$DB_PATH" -nv
DB_HASH=($(sha1sum $DB_PATH | xxd -r -p | base64 | tr '/+' '_-' | tr -d '='))

# Optimize the DB
# Steps from https://github.com/phiresky/sql.js-httpvfs
sqlite3 "$DB_PATH" "PRAGMA journal_mode = DELETE" "PRAGMA page_size = $PAGE_SIZE" "VACUUM"

# Split the DB
SPLIT_DIR="$OUT_DIR/$DB_HASH-$PAGE_SIZE"
mkdir "$SPLIT_DIR"
/bin/bash "$SCRIPT_DIR/create_db.sh" "$DB_PATH" "$SPLIT_DIR"
rm "$DB_PATH"
