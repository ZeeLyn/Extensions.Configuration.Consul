<template>
  <div>
    <h1>UI</h1>
    <tree
      id="my-tree-id"
      :custom-options="myCustomOptions"
      :custom-styles="myCustomStyles"
      :nodes="treeDisplayData"
    ></tree>

    <sweet-modal
      ref="loading"
      overlay-theme="dark"
      modal-theme="dark"
      :blocking="true"
      :hide-close-button="true"
    >loading</sweet-modal>
    <sweet-modal
      title="New Key/Value"
      ref="create_key"
      overlay-theme="dark"
      modal-theme="dark"
    >
      <div class="row">
        <span>Key or folder</span>
        <input
          type="text"
          @input="input_key"
          v-model="newkey"
          placeholder="To create a folder, end a key with /"
        >
      </div>
      <div class="row" v-if="isFolder==false">
        <span>Value</span>
        <input type="text" v-model="newkey_value">
      </div>
      <a class="btn" slot="button" v-on:click="new_key_submit" color="light-grey">Save</a>
    </sweet-modal>
     <sweet-modal
      :title="update_key"
      ref="update_key"
      overlay-theme="dark"
      modal-theme="dark"
    >
        <input type="text" v-model="update_key_value">
      <a class="btn" slot="button" v-on:click="update_key_submit" color="light-grey">Save</a>
    </sweet-modal>
  </div>
</template>

<script>
import Vue from "vue";
import Tree from "@/components/vuejs-tree";
const axios = require("axios");
import { SweetModal, SweetModalTab } from "sweet-modal-vue";

export default {
  name: "ui",
  mounted: function() {
    this.load();
  },
  data: function() {
    return {
      newkey: null,
      newkey_value: null,
      update_key:null,
      update_key_value:null,
      isFolder: false,
      treeDisplayData: [],
      currentParentNode:null,
      currentNode:null
    };
  },
  methods: {
    load: function() {
      var self = this;
      axios.get("http://localhost:5342/key/nodes").then(function(res) {
        self.treeDisplayData = res.data;
      });
    },
    open_new_key: function(parentNode,node) {
      this.currentNode=node;
      this.$refs.create_key.open();
    },
    input_key: function(e) {
      if (!e.data) {
        this.isFolder = false;
        return;
      }
      var d = e.data.length - 1;
      var folder = d >= 0 && e.data.lastIndexOf("/") == d;
      this.isFolder = folder;
    },
    new_key_submit: function() {
      console.log(this.currentNode.id);
      this.$refs.create_key.close();
      this.$refs.loading.open();
      var key=this.currentNode.id+this.newkey;
      axios.put("http://localhost:5342/key/put",{
        key:key,
        value:this.newkey_value
      }).then(res=>{
        if(!this.currentNode.nodes)
          this.currentNode.nodes=[];
          this.currentNode.nodes.push({
            id:key,
            name:this.newkey,
            text:this.newkey_value,
            state:{
              expanded:true
            },
            type:2
          });
        this.$refs.loading.close();
      });
    },
    open_update_key:function(node){
        this.update_key=node.name;
        this.update_key_value=node.text;
        this.$refs.update_key.open();
    },
    update_key_submit:function(){

    }
  },
  components: {
    tree: Tree,
    SweetModal,
    SweetModalTab
  },
  computed: {
    myCustomStyles() {
      return {
        tree: {
          height: "auto",
          maxHeight: "none",
          overflowY: "none",
          display: "block"
        },
        row: {
          width: "100%",
          cursor: "pointer",
          child: {
            height: "35px"
          }
        },
        expanded: {
          class: "icon-closed"
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
        addNode: {
          state: true,
          fn: this.open_new_key,
          appearOnHover: false
        },
        editNode: {
          state: true,
          fn: this.open_update_key,
          appearOnHover: false
        },
        deleteNode: {
          state: true,
          fn: function(parent, node) {
            for (var i = 0; i < parent.nodes.length; i++) {
              console.log(parent.nodes[i].id);
              if (parent.nodes[i].id == node.id) {
                parent.nodes.splice(i, 1);
                break;
              }
            }
          },
          appearOnHover: false
        },
        showTags: true
      };
    }
  }
};
</script>
<style>
* {
  font-family: Arial, Helvetica, sans-serif;
}
@font-face {
  font-family: "icomoon";
  src: url(data:application/font-woff2;charset=utf-8;base64,d09GMgABAAAAAAQkAA0AAAAACQAAAAPQAAEAAAAAAAAAAAAAAAAAAAAAAAAAAAAAP0ZGVE0cGh4GYACCXhEICoZchVwLFgABNgIkAyQEIAWDGwdtG4EHUVRQqsh+HtjOqM8Mi8GbRpuyw2DwW7YqQThcIojG/jd7d98ElQbJTJqoJYvmkQyhSdSkUj1B5D+V3aZfuP8gBTGFQUz9vF6+VOQKKDih3Kj81j9rAY3SZAzL7dV81wSMtf2f+/m8CtCd00U2GuF9i6IKGu6MCrMBpwMZ0GaZGqX57ZtMP+MfnDfwIEKE2+N3QsDru/1q4d1N941zBxrRGUkIdUJBQs6P44Iq0s30HOdBJ/HT0KhjqItWPSeJ7yU/qjzCpIzoCDiSfQXyYXsKsEoW6KkYh3q1kiT0/V4q/ysVSLG+SUXVurJQDyBkoprIQCQk6D8TFOCHhahUtIB60BHNRFfiGKAAAiGEKlmInPMXE1dtI8tkFxcPO3sC3KPVRNtIywRMtYy1TVtJzBJhlAyxHsK2IZnyzSNx77A1iSqGZoaGHEMxRiRZPrISeKlxMYoZnSSaKEJmgo+hgxDZExkWc9fPnRHaCW0ShTbFrcOUI7TYtzDDH2Fd9CnTHuF1sGshQ62YfHvl0KHeX9MbOObzLT0Q/fHGeQQHzkAVUK9rdtI5avUINkl7jCE2KZIxzDVD0UdR0UzwlY3s4u5TWVoyNr2G97p8dJTYZm5gny2NXlI2sm1o8XPDItVgaYzTAvaRkVD0zRcSBEAQRTQUSyjw/xcb//V4CTlGpadFOYaYQ8fzuQiKEgohwQlJxiV8/aEptfr8NvJNTSobU1MpOjWtv2+Y9vZjP8jKMdpRDgQBQJK7e0daCDRZ9gZHl4uMTkuLlkelez5YG02dUVYtVBPTnvpqd4kLvQq9m5u9A7tYnJgLqaWQNiOu66v7GpSd4JzoIpO5JDonZGe7EEIKeSg3k10XWeF853tXV2sjASIRYqmDhcrd4aOqYvDrq9ucv9LL7AWe6s00818HA3igB94qXFEJTV9dRNvGWZtntfX1W3cN3hwRn/Z8AgJoy/v76pZx/wxULG+rqWjKzg9RK4FgSaIrx4GWXP0PgdoCiOVAQmFSOe5qYUJ0Nkmg3ilVILF+Q7N7AoQarQIkNfoLkNWYJEBRY5UAVRocEaBajSsC1OvuhSxKndBMikq4rqiMG4QquFmcKp2tsVTX4U4vWW+YO7Zab7eddtttF1t3a5bdxqQlNtrsoB3W2seXd83oWyRSXWGjffbbmtrXYAMM4prc2uABg8Y5zcgnHtDXZhvtmkVwrQM22hC/zlF9zUa4+YEBvAjknKICs9fv/mfFNkis/euJJEWWo0RVVEdN1FZv3nF0z5bBNQd3bR0EdCFt+uhBg3CDcUNwQ63DAA==)
      format("woff2"),
    url(data:application/font-woff;charset=utf-8;base64,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)
      format("woff");
  font-weight: normal;
  font-style: normal;
}
[class^="icon-"],
[class*=" icon-"],
[class*="_icon"],
.expanded {
  font-family: "icomoon" !important;
  speak: none;
  font-style: normal;
  font-weight: normal;
  font-variant: normal;
  text-transform: none;
  line-height: 1;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}

.row {
  margin: 10px 0;
}
.btn {
  width: 100px;
  border: 1px #273442 solid;
  padding: 5px 20px;
  margin: 10px 0;
  cursor: pointer;
}
input[type="text"] {
  border: none;
  flex: 1;
  padding: 10px 0;
  text-indent: 10px;
  width: 100%;
  font-size: 20px;
}
.row_data {
  display: flex;
  align-items: center;
  border-bottom: 0.5px #eee solid;
}
span {
  display: flex;
  align-items: center;
}
.row_data:hover {
  background: #eee;
}

* {
  margin: 0;
  padding: 0;
}
.icon-closed:before {
  content: "\e903";
  font-size: 15px;
}
.expanded:before {
  content: "\e904";
  font-size: 15px;
}
.add_icon:before {
  content: "\e900";
  font-size: 14px;
  font-weight: bold;
}
.edit_icon::before {
  content: "\e901";
  font-size: 14px;
  font-weight: bold;
}
.delete_icon::before {
  content: "\e902";
  font-size: 16px;
  font-weight: bold;
}

.small-tree-indent {
  width: 16px;
  height: 16px;
  margin-right: 5px;
}
.icon-panel {
  margin-left: 20px;
}
.icon_parent {
  display: flex;
  align-items: center;
  margin-left: 5px;
}
.icon-expanded {
  margin-right: 5px;
}
</style>
