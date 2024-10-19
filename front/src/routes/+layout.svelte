<script>
	import '$lib/css/matcha.css';
	import '$lib/css/style.css';
	import { version } from '$app/environment';
	import { Github } from 'lucide-svelte';
</script>

<slot />
<footer>
	<div style="text-align:right;">
		<i>v{version}</i>
	</div>
</footer>
