<template>
    <ModalFrame ref="modal" id="overview">
        <div slot="title">
            <h3>{{$t("Pages.InterviewOverview")}}</h3>
        </div>

        <OverviewItem v-for="item in items" :key="item.id" :item="item" @showAdditionalInfo="onShowAdditionalInfo" />

        <infinite-loading ref="loader" v-if="overview.total > 0 && items.length > 0" @infinite="infiniteHandler" :distance="1000">
            <span slot="no-more"></span>
            <span slot="no-results"></span>
        </infinite-loading>

        <div slot="actions">
            <button type="button" class="btn btn-link" @click="hide">{{ $t("Pages.CloseLabel") }}</button>
        </div>
    </ModalFrame>
</template>

<script>
import InfiniteLoading from "vue-infinite-loading";
import OverviewItem from "./components/OverviewItem";
import vue from "vue";

export default {
    components: { InfiniteLoading, OverviewItem },
    data: function() {
        return {
            loaded: 100,
            sticked: [],
            scroll: 0,
            scrollable: null,
            itemWithAdditionalInfo: null
        };
    },
    computed: {
        overview() {
            return this.$store.state.review.overview;
        },

        items() {
            return _.slice(this.overview.entities, 0, this.loaded);
        }       
    },
    watch: {
        "overview.isLoaded"(to, from) {
            if (from == true && to == false) {
                this.loaded = 100;
            }
        }
    },
    methods: {
        hide() {
            document.removeEventListener("scroll", this.handleScroll);
            $(this.$refs.modal).modal("hide");
        },

        async show() {
            this.$store.dispatch("loadOverviewData");
            document.addEventListener("scroll", this.handleScroll);

            this.$refs.modal.modal({
                backdrop: 'static',
                keyboard: false
            });
        },

        infiniteHandler($state) {
            const self = this;

            self.loaded += 500;

            $state.loaded();
            if (self.overview.isLoaded && self.loaded >= self.overview.total) {
                $state.complete();
            }
        },

        handleScroll(args, a, b, c) {
            this.scroll = window.scrollY;
        },

        onShowAdditionalInfo(itemToShow)
        {
            if (this.itemWithAdditionalInfo)
            {
                this.itemWithAdditionalInfo.hideAdditionalDetails();
            }
            this.itemWithAdditionalInfo = itemToShow;
        }
    }
};
</script> 