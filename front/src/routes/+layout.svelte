<script>
	import '$lib/css/matcha.css';
	import '$lib/css/style.css';
	import { version } from '$app/environment';
	import { Github } from 'lucide-svelte';
</script>

<header>
	<div style="text-align:right;">
		<i>v{version}</i>
	</div>
</header>
<slot />
