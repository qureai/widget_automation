const edge = require('electron-edge-js');

// Create the edge function
const radWhereFunction = edge.func({
    assemblyFile: 'bin/Debug/PowerScribeBridge.dll',
    typeName: 'PowerScribeBridge.Startup'
});

// DOM elements
const statusDiv = document.getElementById('status');
const connectBtn = document.getElementById('connectBtn');
const eventLog = document.getElementById('eventLog');

let isConnected = false;

// Handle connect button click
connectBtn.addEventListener('click', async () => {
    try {
        if (!isConnected) {
            console.log('Attempting to connect...');
            
            const input = {
                action: 'connect',
                callback: function(data) {
                    console.log('Received event from C#:', data);
                    addEventToLog(data);
                }
            };
            
            console.log('Calling edge function with input:', input);
            
            const result = await new Promise((resolve, reject) => {
                radWhereFunction(input, (error, result) => {
                    if (error) {
                        console.error('Edge function error:', error);
                        reject(error);
                    } else {
                        console.log('Edge function result:', result);
                        resolve(result);
                    }
                });
            });

            console.log('Connect result:', result);

            if (!result) {
                throw new Error('No result returned from edge function');
            }

            if (result.success) {
                isConnected = true;
                statusDiv.textContent = `Status: Connected - ${result.message || 'Success'}`;
                connectBtn.textContent = 'Disconnect';
            } else {
                throw new Error(result.error || 'Unknown error occurred');
            }
        } else {
            console.log('Attempting to disconnect...');
            
            const input = {
                action: 'disconnect',
                callback: null
            };
            
            console.log('Calling edge function with input:', input);
            const result = await new Promise((resolve, reject) => {
                radWhereFunction(input, (error, result) => {
                    if (error) {
                        console.error('Edge function error:', error);
                        reject(error);
                    } else {
                        console.log('Edge function result:', result);
                        resolve(result);
                    }
                });
            });

            console.log('Disconnect result:', result);

            if (!result) {
                throw new Error('No result returned from edge function');
            }

            if (result.success) {
                isConnected = false;
                statusDiv.textContent = `Status: Disconnected - ${result.message || 'Success'}`;
                connectBtn.textContent = 'Connect to RadWhere';
            } else {
                throw new Error(result.error || 'Unknown error occurred');
            }
        }
    } catch (error) {
        console.error('Error in button click handler:', error);
        statusDiv.textContent = `Status: Error - ${error.message}`;
    }
}); 