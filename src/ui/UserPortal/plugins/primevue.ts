import PrimeVue from 'primevue/config';
import Button from 'primevue/button';
import InputText from 'primevue/inputtext';
import Dialog from 'primevue/dialog';
import Toast from 'primevue/toast';
import Chip from 'primevue/chip';
import ToastService from 'primevue/toastservice';
import Tooltip from 'primevue/tooltip';
import Dropdown from 'primevue/dropdown';
import { defineNuxtPlugin } from '#app';

export default defineNuxtPlugin((nuxtApp) => {
	nuxtApp.vueApp.use(PrimeVue, { ripple: true });
	nuxtApp.vueApp.component('Button', Button);
	nuxtApp.vueApp.component('InputText', InputText);
	nuxtApp.vueApp.component('Dialog', Dialog);
	nuxtApp.vueApp.component('Toast', Toast);
	nuxtApp.vueApp.component('Chip', Chip);
	nuxtApp.vueApp.component('Dropdown', Dropdown);

	nuxtApp.vueApp.use(ToastService);
	nuxtApp.vueApp.directive('tooltip', Tooltip);
});
