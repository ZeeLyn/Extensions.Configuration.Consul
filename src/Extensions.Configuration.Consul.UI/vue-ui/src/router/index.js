import Vue from 'vue'
import Router from 'vue-router'
import HelloWorld from '@/components/HelloWorld'
import ui from '@/views/ui'


Vue.use(Router)

export default new Router({
    routes: [{
            path: '/',
            name: 'ui',
            component: ui
        },
        {
            path: '/',
            name: 'HelloWorld',
            component: HelloWorld
        }
    ]
})