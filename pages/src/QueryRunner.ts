import { createDbWorker, WorkerHttpvfs } from "sql.js-httpvfs"
import { SplitFileConfig } from "sql.js-httpvfs/dist/sqlite.worker";

const workerUrl = new URL("sql.js-httpvfs/dist/sqlite.worker.js", import.meta.url);
const wasmUrl = new URL("sql.js-httpvfs/dist/sql-wasm.wasm", import.meta.url);

export enum DbType {
    Normalized = "Normalized",
    Denormalized = "Denormalized",
}

const workers = new Map<DbType, WorkerHttpvfs>();

export async function execute(query: string, dbType: DbType) {

    let worker = workers.get(dbType);
    if (!worker) {

        const config: SplitFileConfig = {
            from: "jsonconfig",
            // This will run from ./static/js and needs to reach ./data/config.json
            configUrl: `../../data/${dbType.toLowerCase()}/${process.env['DB_SUBDIR_' + dbType.toUpperCase()]}/config.json`
        }

        worker = await createDbWorker(
            [config],
            workerUrl.toString(),
            wasmUrl.toString()
        )

        workers.set(dbType, worker)
    }

    const db: any = worker.db;
    return await db.exec(query);
}
