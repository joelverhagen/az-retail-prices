#!/bin/bash

SCRIPT_DIR="$(dirname "$0")"
DB_URL="https://github.com/joelverhagen/data-az-retail-prices/releases/download/latest/azure-prices.db"
DEFAULT_OUT_DIR="$SCRIPT_DIR/build/data"

OUT_DIR="${1:-$DEFAULT_OUT_DIR}"
DB_PATH="$OUT_DIR/azure-prices.db"

# Set up the data directory
rm -rf "$OUT_DIR"
mkdir "$OUT_DIR" --parents

# Download the latest DB
wget "$DB_URL" -O "$DB_PATH" -nv

# Optimize the DB
# Steps from https://github.com/phiresky/sql.js-httpvfs
sqlite3 "$DB_PATH" "PRAGMA journal_mode = DELETE" "PRAGMA page_size = 32768" "VACUUM"

# Split the DB
/bin/bash "$SCRIPT_DIR/create_db.sh" "$DB_PATH" "$OUT_DIR"
rm "$DB_PATH"
