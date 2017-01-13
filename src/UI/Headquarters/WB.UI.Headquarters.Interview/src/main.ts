
// tslint:disable-next-line:ordered-imports
import * as jQuery from "jquery"
const $ = (window as any).$ = (window as any).jQuery = jQuery
import "bootstrap-sass/assets/javascripts/bootstrap/modal"

import "es6-promise/auto"
import "jquery-mask-plugin"
import * as Vue from "vue"
import * as Vuex from "vuex"

Vue.use(Vuex)

import "./components/entities"
import "./components/entities/parts"

import "./errors.ts"
import router from "./router"
import store from "./store"

import App from "./App"

const vueApp = new Vue({
  el: "#app",
  template: "<App/>",
  components: { App },
  store,
  router
})
