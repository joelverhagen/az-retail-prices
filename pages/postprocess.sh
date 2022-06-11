#!/bin/bash

SCRIPT_DIR="$(dirname "$0")"
mkdir "$SCRIPT_DIR/build/data" --parents

DB_URL=https://github.com/joelverhagen/data-az-retail-prices/releases/download/latest/azure-prices.db
wget "$DB_URL" -O "$SCRIPT_DIR/build/data/azure-prices.db"
