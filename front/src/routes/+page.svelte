<script lang="ts">
	import { onMount } from 'svelte';
	import { type Message } from '$lib/message';

	onMount(() => {
		window.external.receiveMessage((message: string) => {
			console.log('Received message: ' + message);
			handleMessage(message);
		});
		sendMessage({ command: 'default_map_location' });
	});

	function sendMessage(Message: Message) {
		if (window === undefined || window.external === undefined) {
			console.error('Tried to send a message but the window.external object is not available');
			return;
		}
		console.log('Sending message for real: ' + JSON.stringify(Message));
		window.external.sendMessage(JSON.stringify(Message));
		console.log('Message sent');
	}

	function handleMessage(messageJson: string) {
		const message = JSON.parse(messageJson);
		const data = JSON.parse(message.data);
		console.log('Handling message: ' + JSON.stringify(message));
		switch (message.command) {
			case 'error':
				alert(data);
				break;
			case 'map_info':
				map_info = data;
				break;
			case 'default_map_location':
				map_path = data;
				break;
		}
	}

	let map_path: string = $state('');
	let map_info: any = $state(null);

	function openMap() {
		console.log('Opening map: ' + map_path);
		var message: Message = {
			command: 'open_map',
			data: map_path
		};
		console.log('Sending message: ' + JSON.stringify(message));
		sendMessage(message);
	}
</script>

<div>
	<input type="text" bind:value={map_path} placeholder="Enter a path to a .Map.Gbx file" />
	<button onclick={() => openMap()}>Open Map</button>
</div>

{#if map_info}
	<div>
		<p>Author: {map_info.author}</p>
		<p>Name: {map_info.name}</p>
		<p>X:{map_info.size.x}</p>
		<p>Y:{map_info.size.y}</p>
		<p>Z:{map_info.size.z}</p>
	</div>
{/if}
