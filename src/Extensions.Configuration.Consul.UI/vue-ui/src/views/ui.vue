<template>
  <div>
    <h1>UI</h1>
    <tree
      id="my-tree-id"
      :custom-options="myCustomOptions"
      :custom-styles="myCustomStyles"
      :nodes="treeDisplayData"
    ></tree>
  </div>
</template>

<script>
import Vue from "vue";
import Tree from "vuejs-tree";
export default {
  name: "ui",
  data() {
    return {
      treeDisplayData: [
        {
          text: "Root 1",
          nodes: [
            {
              text: "Child 1",
              nodes: [
                {
                  text: "Grandchild 1"
                },
                {
                  text: "Grandchild 2"
                }
              ]
            },
            {
              text: "Child 2"
            }
          ]
        },
        {
          text: "Root 2"
        }
      ]
    };
  },
  methods: {
    getTree(treeId) {
      for (let i = 0; i < this.$children.length; i++) {
        if (this.$children[i].$props.id == treeId) return this.$children[i];
      }
    }
  },
  components: {
    tree: Tree
  },
  computed: {
    myCustomStyles() {
      return {
        tree: {
          height: "auto",
          maxHeight:"none",
          overflowY: "none",
          display: "block"
        },
        row: {
          width: "100%",
          cursor: "pointer",
          child: {
            height: "35px"
          }
        }
      };
    },
    myCustomOptions() {
      return {
        treeEvents: {
          expanded: {
            state: true,
            fn: null
          },
          collapsed: {
            state: false,
            fn: null
          },
          selected: {
            state: false,
            fn: null
          },
          checked: {
            state: true,
            fn: this.myCheckedFunction
          }
        },
        events: {
          expanded: {
            state: true,
            fn: {}
          },
          selected: {
            state: false,
            fn: null
          },
          checked: {
            state: false,
            fn: null
          },
          editableName: {
            state: false,
            fn: null,
            calledEvent: null
          }
        },
        addNode: { state: true, fn: {}, appearOnHover: false },
        editNode: { state: true, fn: {}, appearOnHover: true },
        deleteNode: { state: true, fn: {}, appearOnHover: true },
        showTags: true
      };
    }
  }
};
</script>
<style>
.custom_class {
  height: 30px;
  width: 30px;
}
</style>
