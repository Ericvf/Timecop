class BlazorClientStorage {
    open(name, version, data) {
        if (this.idbDatabase)
            return Promise.resolve();

        return new Promise((resolve, reject) => {
            const dbOpenDBRequest = window.indexedDB.open(name, version);
            dbOpenDBRequest.onerror = (event) => reject(event);
            dbOpenDBRequest.onsuccess = () => {
                this.idbDatabase = dbOpenDBRequest.result;
                resolve(dbOpenDBRequest.result);
            };
            dbOpenDBRequest.onupgradeneeded = () => this.handleOnUpgradeNeeded(dbOpenDBRequest.result, data);
        });
    }

    handleOnUpgradeNeeded(idbDatabase, data) {
        const handleOnUpgradeNeededEvent = new CustomEvent('BlazorClientStorage.handleOnUpgradeNeeded', { detail: { idbDatabase, data } });
        window.dispatchEvent(handleOnUpgradeNeededEvent);

        for (const storeData of data) {
            const store = idbDatabase.createObjectStore(storeData.name, storeData.options);
            if (storeData.indices) {
                for (const index of storeData.indices) {
                    store.createIndex(index.name, index.name, index.options);
                }
            }
        }
    }

    executeWithTransaction(storeName, action) {
        return new Promise((resolve, reject) => {
            const transaction = this.idbDatabase.transaction(storeName, "readwrite");
            transaction.onerror = e => { reject(e.target.error.message); }
            transaction.oncomplete = _ => { resolve(); }

            const store = transaction.objectStore(storeName);
            action(store, transaction);
        });
    }

    add(storeName, model) {
        return new Promise((resolve, reject) => {
            const transaction = this.idbDatabase.transaction(storeName, "readwrite");
            const store = transaction.objectStore(storeName);

            delete model.key;
            const request = store.add(model);
            transaction.onerror = e => { reject(e.target.error.message); }
            transaction.oncomplete = _ => {
                resolve(request.result);
            };
        });
    }

    put(storeName, model, keyed) {
        return this.executeWithTransaction(storeName, (store, transaction) => {
            if (keyed) {
                store.put(model);
            }
            else {
                store.put(model, model.key);
            }
        });
    }

    get(storeName, key) {
        return new Promise((resolve, reject) => {
            const transaction = this.idbDatabase.transaction(storeName, "readonly");
            const store = transaction.objectStore(storeName);

            var request = store.get(key);
            request.onerror = (e) => { reject(e.target.error.message); }
            request.onsuccess = (e) => {
                const model = e.target.result || {};
                model.key = key;
                resolve(model);
            }
        });
    }

    delete(storeName, key) {
        return this.executeWithTransaction(storeName, (store, transaction) => {
            store.delete(key);
        });
    }

    getAll(storeName) {
        return new Promise((resolve, reject) => {
            const transaction = this.idbDatabase.transaction(storeName, "readonly");
            const store = transaction.objectStore(storeName);
            var request = store.getAll();
            request.onerror = (e) => { reject(e.target.error.message); }
            request.onsuccess = (e) => resolve(e.target.result);
        });
    }

    getByIndex(storeName, indexName, value) {
        return new Promise((resolve, reject) => {
            const transaction = this.idbDatabase.transaction(storeName, "readonly");
            const store = transaction.objectStore(storeName);

            var indexStore = store.index(indexName);
            var cursor = indexStore.openCursor(value);
            var results = [];

            cursor.onerror = (e) => reject(e.target.error.message);
            cursor.onsuccess = function (event) {
                cursor = event.target.result;
                if (cursor) {
                    var item = cursor.value;
                    item.key = cursor.primaryKey;
                    results.push(item);

                    cursor.continue();
                } else {
                    resolve(results);
                }
            };
        });
    }

    openCursor(storeName) {
        return new Promise((resolve, reject) => {
            const transaction = this.idbDatabase.transaction(storeName, "readonly");
            const store = transaction.objectStore(storeName);

            var results = [];
            var cursor = store.openCursor();
            cursor.onerror = (e) => reject(e.target.error.message);
            cursor.onsuccess = function (event) {
                cursor = event.target.result;
                if (cursor) {
                    const model = cursor.value;
                    model.key = cursor.key;
                    results.push(model);
                    cursor.continue();
                } else {
                    resolve(results);
                }
            };
        });
    }
}

window.BlazorClientStorage = new BlazorClientStorage();