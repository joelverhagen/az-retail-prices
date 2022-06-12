import { createDbWorker, WorkerHttpvfs } from "sql.js-httpvfs"
import { SplitFileConfig } from "sql.js-httpvfs/dist/sqlite.worker";

const workerUrl = new URL("sql.js-httpvfs/dist/sqlite.worker.js", import.meta.url);
const wasmUrl = new URL("sql.js-httpvfs/dist/sql-wasm.wasm", import.meta.url);

let worker: WorkerHttpvfs;

export async function execute(query: string) {
    if (!worker) {

        const config: SplitFileConfig = {
            from: "jsonconfig",
            // This will run from ./static/js and needs to reach ./data/config.json
            configUrl: `../../data/${process.env.DB_SUBDIR}/config.json`
        }

        worker = await createDbWorker(
            [config],
            workerUrl.toString(),
            wasmUrl.toString()
        );
    }

    const db: any = worker.db;
    return await db.exec(query);
}
