<template>
	<main>
		<Head>
			<Title>{{ pageTitle }}</Title>
			<Meta name="description" :content="pageTitle" />
		</Head>

		<NuxtPage :style="style" />
	</main>
</template>

<script lang="ts">
import { mapStores } from 'pinia';
import { useAppConfigStore } from '@/stores/appConfigStore';

export default {
	data() {
		return {
			pageTitle: 'FoundationaLLM',
		};
	},

	computed: {
		...mapStores(useAppConfigStore),

		style() {
			return {
				'--primary-bg': this.appConfigStore.primaryBg,
				'--primary-color': this.appConfigStore.primaryColor,
				'--secondary-color': this.appConfigStore.secondaryColor,
				'--accent-color': this.appConfigStore.accentColor,
				'--primary-text': this.appConfigStore.primaryText,
				'--secondary-text': this.appConfigStore.secondaryText,
			};
		},
	},
};
</script>

<style lang="scss">
html,
body,
#__nuxt,
#__layout,
main {
	height: 100%;
	margin: 0;
	font-family: 'Poppins', sans-serif;
}
</style>
