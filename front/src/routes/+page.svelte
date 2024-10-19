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
			case 'image_info':
				image_info = data;
				break;
			case 'default_map_location':
				map_path = data;
				break;
		}
	}

	let map_path: string = $state('');
	let map_info: any = $state(null);
	let images: FileList | null = $state(null);
	let image_info: any = $state(null);

	$effect(() => {
		if (images && (images as FileList).length > 0) {
			const file = images[0];
			if (!file) {
				console.error('No file selected');
			}
			console.log(file);
			console.log('Opening image: ' + file.name);
			const reader = new FileReader();
			reader.onload = async (e) => {
				var message = {
					command: 'open_image',
					data: String(reader.result).split(',')[1]
				};
				sendMessage(message);
			};
			reader.readAsDataURL(file);
		}
	});

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

<main>
	<fieldset>
		<legend>Map</legend>
		<label for="map_path">Enter a path to a .Map.Gbx file</label>
		<input type="text" bind:value={map_path} placeholder="Enter a path to a .Map.Gbx file" />
		<button type="button" onclick={() => openMap()}>Open Map</button>

		{#if map_info}
			<div class="flex">
				<p>Author: {map_info.author}</p>
				<p>Name: {map_info.name}</p>
				<p>X:{map_info.size.x}</p>
				<p>Y:{map_info.size.y}</p>
				<p>Z:{map_info.size.z}</p>
			</div>
		{/if}
	</fieldset>

	{#if map_info}
		<fieldset>
			<legend>Image</legend>
			<label for="image">Choose an image</label>
			<input type="file" id="image" accept="image/*" bind:files={images} />
			{#if image_info}
				<div class="flex">
					<p>Width: {image_info.width}</p>
					<p>Height: {image_info.height}</p>
					<p>Blocks: {image_info.blockCount}</p>
				</div>
				<p>Preview:</p>
				<img src={image_info.preview} alt="Preview" />
				<div>
					<button onclick={() => sendMessage({ command: 'draw', data: '' })}>Draw</button>
				</div>
			{/if}
		</fieldset>
	{/if}

	{#if map_info}
		<hr />
		<button onclick={() => sendMessage({ command: 'wipe', data: '' })} type="reset"
			>Wipe All Pixel Items From Map</button
		>
		<div>
			<button onclick={() => sendMessage({ command: 'save', data: '' })} type="submit"
				>Save Map</button
			>
		</div>
	{/if}
</main>
