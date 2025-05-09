// File: Plugins/WebGL/WebSocketPlugin.jslib
mergeInto(LibraryManager.library, {
    WS_Create: function(urlPtr, receiverPtr, onMessageRecievedCallback, onConnectCallback, onSocketClosedCallback) {
        // Safely convert pointers to strings
        var url, receiver, onMessageRecievedCallbackPtr, onConnectCallbackPtr, onSocketClosedCallbackPtr;
        try {
            if (!urlPtr) throw 'NULL urlPtr';
            url = UTF8ToString(urlPtr);
        } catch (e) {
            console.error('WS_Create: invalid urlPtr', urlPtr, e);
            return;
        }
        try {
            if (!receiverPtr) throw 'NULL receiverPtr';
            receiver = UTF8ToString(receiverPtr);
        } catch (e) {
            console.error('WS_Create: invalid receiverPtr', receiverPtr, e);
            return;
        }
        try {
            if (!onMessageRecievedCallback) throw 'NULL onMessageRecievedCallbackPtrPtr';
            onMessageRecievedCallbackPtr = UTF8ToString(onMessageRecievedCallback);
        } catch (e) {
            console.error('WS_Create: invalid onMessageRecievedCallbackPtrPtr', onMessageRecievedCallback, e);
            return;
        }
        try {
            if (!onConnectCallback) throw 'NULL onConnectCallback';
            onConnectCallbackPtr = UTF8ToString(onConnectCallback);
        } catch (e) {
            console.error('WS_Create: invalid onConnectCallback', onConnectCallback, e);
            return;
        }
        try {
            if (!onSocketClosedCallback) throw 'NULL onConnectCallback';
            onSocketClosedCallbackPtr = UTF8ToString(onSocketClosedCallback);
        } catch (e) {
            console.error('WS_Create: invalid onConnectCallback', onSocketClosedCallback, e);
            return;
        }

        if (this.ws) { this.ws.close(); this.ws = null; }
        try {
            this.ws = new WebSocket(url);
            this.ws.binaryType = 'arraybuffer';

            this.ws.onopen = function() {
                console.log('WebSocket connected to ' + url);
                SendMessage(receiver, onConnectCallbackPtr);
            };

			this.ws.onmessage = function(event) {
				console.log(event.data);
				// If event.data is already a string, no need to convert it to Uint8Array
				var base64String = btoa(event.data);  // Encode the string as Base64
				SendMessage(receiver, onMessageRecievedCallbackPtr, base64String);
			};

            this.ws.onclose = function(e) {
                console.log('WebSocket closed:', e.code, e.reason);
                SendMessage(receiver, onSocketClosedCallbackPtr);
            };
            this.ws.onerror = function(e) {
                console.error('WebSocket error', e);
                SendMessage(receiver, onSocketClosedCallbackPtr);
            };
        } catch (ex) {
            console.error('WebSocket connection failed:', ex);
        }
    },

    WS_Send: function(dataPtr, length) {
        if (!this.ws || this.ws.readyState !== WebSocket.OPEN) {
            console.error('WebSocket is not open. Cannot send data.');
            return;
        }
        try {
            var data = HEAPU8.subarray(dataPtr, dataPtr + length);
            this.ws.send(data);
        } catch (ex) {
            console.error('WebSocket send failed:', ex);
        }
    },

    WS_Close: function() {
        if (this.ws) {
            this.ws.close();
            this.ws = null;
        }
    }
});