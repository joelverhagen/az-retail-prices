#!/bin/bash

SCRIPT_DIR=$(dirname "$0")
OUT_DIR=$SCRIPT_DIR/build/data
DB_URL=https://github.com/joelverhagen/data-az-retail-prices/releases/download/latest/azure-prices.db
DB_PATH=$SCRIPT_DIR/build/data/azure-prices.db

# Set up the data directory
rm -rf "$OUT_DIR"
mkdir "$SCRIPT_DIR/build/data" --parents

# Download the latest DB
wget "$DB_URL" -O "$DB_PATH" -nv

# Optimize the DB
# Steps from https://github.com/phiresky/sql.js-httpvfs
sqlite3 "$DB_PATH" "pragma journal_mode = delete"
sqlite3 "$DB_PATH" "pragma page_size = 1024"
sqlite3 "$DB_PATH" "vacuum"

# Split the DB
/bin/bash "$SCRIPT_DIR/create_db.sh" "$DB_PATH" "$OUT_DIR"
rm "$DB_PATH"
