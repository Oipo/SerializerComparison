'use strict';
const nc = require('nodeaffinity');
const wincmd = require('node-windows');
const exec = require('child_process').exec;
const personClass = require('./person').Person;
const documentClass = require('./person').Document;
const testRunnerClass = require('./testRunner').TestRunner;
const _ = require('lodash/core');
const fs = require('fs');
const winston = require('winston');
const fastjsonparse = require('fast-json-parse');
const fastjsonstringifyClass = require('fast-json-stringify');
const fastjsonClass = require('fast-json');

function setPriority(callback) {
    exec('wmic process where name="node.exe" CALL setpriority "High Priority"', function (err, so, se) {
        if (err) {
            console.error(`exec error: ${err}`);
            callback(false);
        } else {
            callback(true);
        }
    });
}

function printMeasurements(name, measurements) {
    let min = _.min(measurements);
    let max = _.max(measurements);
    let avg = _(measurements).reduce(function (a, m, i, p) {
        return a + m / p.length;
    }, 0);

    console.log(`${name} - min ${min} µs - max ${max} µs - avg ${avg} µs`);
}

let documents = [];

for (let i = 0; i < 1000; i++) {
    let date = new Date();
    date.setDate(date.getDate() + i);
    documents.push(new documentClass(i, `License${i}`, 'abcdefghijklmnopqrstuvwxyzüäçéèß', date));
}

let person = new personClass(123, new Date(), 'John Doe', documents);

winston.add(winston.transports.File, {
    filename: 'measurements.txt',
    timestamp: () => {
        return Date.now();
    },
    formatter: (options) => {
        // the pipes are here so that BoxPlotter picks up the measurements correctly
        return options.timestamp() + '|' + options.level.toUpperCase() + '||' + (options.message !== undefined ? options.message : '');
    },
    json: false
});
winston.remove(winston.transports.Console);
winston.level = 'debug';

wincmd.isAdminUser((isAdmin) => {
    if (!isAdmin) {
        console.log('Please run this as administrator');
        winston.log('Not running as admin');
        process.exit(1);
    }

    if (!nc.setAffinity(1)) {
        console.log("Couldn't set processor affinity");
        winston.log("Couldn't set processor affinity");
        process.exit(1);
    }

    setPriority((success) => {
        if (success) {
            let testRunner = new testRunnerClass();
            let measurements = [];
            let fastjson = new fastjsonClass();
            let fastjsonstringify = new fastjsonstringifyClass({
                title: 'person json',
                type: 'object',
                properties: {
                    age: {
                        type: 'number'
                    },
                    birthday: {
                        type: 'string'
                    },
                    name: {
                        type: 'string'
                    },
                    documents: {
                        title: 'document json',
                        type: 'object',
                        properties: {
                            id: {
                                type: 'number'
                            },
                            name: {
                                type: 'string'
                            },
                            content: {
                                type: 'string'
                            },
                            expirationDate: {
                                type: 'string'
                            }
                        }
                    }
                }
            });
            // When starting with Visual Studio, the working directory is the same as the C# project.
            let personFileContents = fs.readFileSync('..\\SerializerComparison\\PersonJson.txt', 'UTF8');

            winston.info('Starting measurements');
            
            for (let i = 0; i < 250; i++) {
                measurements.push(testRunner.run(() => {
                    JSON.stringify(person);
                }));
            }

            printMeasurements('JSON.stringify', measurements);
            winston.debug(`JSON.stringify: ${measurements.join(':')}`);
            measurements = [];

            for (let i = 0; i < 250; i++) {
                measurements.push(testRunner.run(() => {
                    JSON.parse(personFileContents);
                }));
            }

            printMeasurements('JSON.parse', measurements);
            winston.debug(`JSON.parse: ${measurements.join(':')}`);
            measurements = [];

            for (let i = 0; i < 250; i++) {
                measurements.push(testRunner.run(() => {
                    // doesn't deserialize documents :(
                    let result = fastjsonparse(personFileContents);
                    if (result.err) {
                        console.log(`fast-json-parse fail: ${result.err}`);
                    }
                }));
            }

            printMeasurements('fast-json-parse', measurements);
            winston.debug(`fast-json-parse: ${measurements.join(':')}`);
            measurements = [];

            for (let i = 0; i < 250; i++) {
                measurements.push(testRunner.run(() => {
                    // seems to not stringify anything, hence the really fast times
                    let result = fastjsonstringify(person);
                }));
            }

            printMeasurements('fast-json-stringify', measurements);
            winston.debug(`fast-json-stringify: ${measurements.join(':')}`);
            measurements = [];

            for (let i = 0; i < 250; i++) {
                measurements.push(testRunner.run(() => {
                    let result = fastjson.write(personFileContents);
                }));
            }

            printMeasurements('fast-json', measurements);
            winston.debug(`fast-json: ${measurements.join(':')}`);
            measurements = [];

            winston.info('Stopping measurements');
        }
    });
});