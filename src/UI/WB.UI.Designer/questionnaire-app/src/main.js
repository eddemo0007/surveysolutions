import { createApp } from 'vue';
import { createPinia } from 'pinia';
import App from './App.vue';
import router from './router';
import VueDOMPurifyHTML from 'vue-dompurify-html';
import i18n from './plugins/localization';
//import { vuetify /*, install, i18n */ } from './plugins/vuetify';

//import '../../questionnaire/content/markup.less';
//import 'bootstrap/dist/css/bootstrap.min.css';
import 'bootstrap';

import ConfirmDialog from './plugins/confirm';
import PerfectScrollbar from 'vue3-perfect-scrollbar';

//import * as uiv from 'uiv';

import directives from './directives/';

/** Register Vue */
const pinia = createPinia();
const vue = createApp(App);
vue.use(router);
vue.use(pinia);
//vue.use(vuetify); //reqired by options component. consider either remove or use.
vue.use(i18n);
//vue.use(uiv);
vue.use(PerfectScrollbar);
vue.use(VueDOMPurifyHTML);

directives(vue);

// Run!
router.isReady().then(() => {
    vue.mount('#app');
});
