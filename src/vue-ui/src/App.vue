<template>
  <div id="app">
    <header>
      <span>Consul configuration center</span>
    </header>
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
    <sweet-modal title="New Key/Value" ref="create_key" modal-theme="dark">
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
    <sweet-modal :title="update_key" ref="update_key" modal-theme="dark">
      <input type="text" v-model="update_key_value">
      <a class="btn" slot="button" v-on:click="update_key_submit" color="light-grey">Save</a>
    </sweet-modal>

    <sweet-modal ref="success" icon="success" modal-theme="dark">Successfully saved!</sweet-modal>

    <sweet-modal ref="error" icon="error" modal-theme="dark">{{errorMsg}}</sweet-modal>

    <sweet-modal ref="confirm" icon="warning" modal-theme="dark">
      Are you sure you want to delete?
      <a
        class="btn"
        slot="button"
        v-on:click="canncel_delete"
        color="light-grey"
      >No</a>
      <a class="btn" slot="button" v-on:click="delete_key_submit" color="light-grey">Yes</a>
    </sweet-modal>

    <sweet-modal
      title="Set password"
      ref="set_password"
      modal-theme="dark"
      :blocking="true"
      :hide-close-button="true"
    >
      <div class="row">
        <span>Password</span>
        <input type="password" minlength="6" maxlength="18" v-model="password">
      </div>
      <div class="row">
        <span>Re-Enter</span>
        <input type="password" minlength="6" maxlength="18" v-model="reEnter_password">
      </div>
      <span style="color:#D93600;">{{set_password_error_message}}</span>
      <a class="btn" slot="button" v-on:click="set_password" color="light-grey">Save</a>
    </sweet-modal>

    <sweet-modal
      title="Log in"
      ref="login"
      modal-theme="dark"
      :blocking="true"
      :hide-close-button="true"
    >
      <div class="row">
        <span>Password</span>
        <input type="password" minlength="6" maxlength="18" v-model="password">
      </div>
      <span style="color:#D93600;">{{login_error_message}}</span>
      <a class="btn" slot="button" v-on:click="login" color="light-grey">Log in</a>
    </sweet-modal>
  </div>
</template>

<script>
import Vue from "vue";
import Tree from "@/components/vuejs-tree";
const axios = require("axios");
import { SweetModal, SweetModalTab } from "sweet-modal-vue";

export default {
  name: "app",
  mounted: function() {
    this.load();
  },
  data: function() {
    return {
      newkey: null,
      newkey_value: null,
      update_key: null,
      update_key_value: null,
      isFolder: false,
      treeDisplayData: [],
      currentParentNode: null,
      currentNode: null,
      errorMsg: "Operation error!",
      password: null,
      reEnter_password: null,
      set_password_error_message: null,
      login_error_message:null
    };
  },
  methods: {
    load: function() {
      var self = this;
      this.$refs.loading.open();
      axios
        .get("http://localhost:5342/key/nodes", {
          headers: {
            Authorization: "Bearer " + localStorage.getItem("access_token")
          }
        })
        .then(function(res) {
          self.treeDisplayData = res.data;
        })
        .catch(function(err) {
          if (err.response && err.response.status == 401) {
            self.check_auth();
          }
        })
        .then(function() {
          self.$refs.loading.close();
        });
    },
    open_new_key: function(parentNode, node) {
      this.currentNode = node;
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
      this.$refs.create_key.close();
      var hasKey = false;
      if (!this.currentNode.nodes) this.currentNode.nodes = [];
      this.currentNode.nodes.forEach(e => {
        if (e.id == this.currentNode.id + this.newkey) {
          hasKey = true;
          return;
        }
      });
      if (hasKey) {
        this.errorMsg = "Duplicate keys detected!";
        this.$refs.error.open();
        return;
      }
      this.errorMsg = "Operation error!";
      this.$refs.loading.open();
      var key =
        this.currentNode.id +
        (this.currentNode.type == 2 ? ":" : "") +
        this.newkey;
      var self = this;
      axios
        .put("http://localhost:5342/key/put", {
          key: key,
          value: this.newkey_value
        },{
          headers: {
              Authorization: "Bearer " + localStorage.getItem("access_token")
            }
        })
        .then(res => {
          if (!this.currentNode.nodes) this.currentNode.nodes = [];
          this.currentNode.nodes.push({
            id: key,
            name: this.isFolder
              ? this.newkey.substring(0, this.newkey.lastIndexOf("/"))
              : this.newkey,
            text: this.newkey_value,
            state: {
              expanded: true
            },
            type: this.isFolder ? 0 : 2
          });
          self.$refs.success.open();
        })
        .catch(function(res) {
          self.$refs.error.open();
        })
        .then(function() {
          self.$refs.loading.close();
          self.currentNode = null;
          (self.newkey = null), (self.newkey_value = null);
        });
    },
    open_update_key: function(node) {
      this.currentNode = node;
      this.update_key = node.name;
      this.update_key_value = node.text;
      this.$refs.update_key.open();
    },
    update_key_submit: function() {
      this.$refs.update_key.close();
      this.$refs.loading.open();
      var self = this;
      axios
        .put(
          "http://localhost:5342/key/put",
          {
            key: this.currentNode.id,
            value: this.update_key_value
          },
          {
            headers: {
              Authorization: "Bearer " + localStorage.getItem("access_token")
            }
          }
        )
        .then(res => {
          self.currentNode.text = this.update_key_value;
          self.$refs.loading.close();
          self.$refs.success.open();
        })
        .catch(function(res) {
          self.$refs.error.open();
        })
        .then(function() {
          self.$refs.loading.close();
          self.update_key = null;
          self.update_key_value = null;
          self.currentNode = null;
        });
    },
    delete_confirm: function(parentNode, node) {
      this.currentNode = node;
      this.currentParentNode = parentNode;
      this.$refs.confirm.open();
    },
    canncel_delete: function() {
      this.$refs.confirm.close();
      this.currentParentNode = null;
      this.currentNode = null;
    },
    delete_key_submit: function() {
      this.$refs.confirm.close();
      this.$refs.loading.open();
      var self = this;
      var parentNode = this.currentParentNode;
      var node = this.currentNode;
      axios
        .delete("http://localhost:5342/key/delete", {
          data: { key: node.id },
          headers:{
            Authorization: "Bearer " + localStorage.getItem("access_token")
          }
        })
        .then(res => {
          for (var i = 0; i < parentNode.nodes.length; i++) {
            if (parentNode.nodes[i].id == node.id) {
              parentNode.nodes.splice(i, 1);
              break;
            }
          }
          self.$refs.loading.close();
          self.$refs.success.open();
        })
        .catch(function(res) {
          console.error(res);
          self.$refs.error.open();
        })
        .then(function() {
          self.$refs.loading.close();
        });
    },
    check_auth: function() {
      var self = this;
      this.$refs.loading.open();
      axios
        .get("http://localhost:5342/account/check")
        .then(function(res) {
          if (res.data == 0) {
            self.$refs.loading.close();
            self.$refs.set_password.open();
          } else {
             self.$refs.loading.close();
            self.$refs.login.open();
          }
        })
        .catch(function(err) {
          self.$refs.error.open();
        })
        .then(function() {
          self.$refs.loading.close();
        });
    },
    set_password: function() {
      var self = this;
      this.$refs.loading.open();
      axios
        .post("http://localhost:5342/account/set", {
          password: self.password,
          reEnter: self.reEnter_password
        })
        .then(function(res) {
          self.$refs.set_password.close();
          localStorage.setItem("access_token", res.data);
          self.load();
        })
        .catch(function(err) {
          if (err.response && err.response.data)
            self.set_password_error_message = err.response.data;
        })
        .then(function() {
          self.$refs.loading.close();
        });
    },
    login:function(){
      var self=this;
      axios
        .post("http://localhost:5342/account/login", {
          password: self.password
        })
        .then(function(res) {
          self.$refs.login.close();
          localStorage.setItem("access_token", res.data);
          self.load();
        })
        .catch(function(err) {
          if (err.response && err.response.data)
            self.login_error_message = err.response.data;
        })
        .then(function() {

        });
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
          appearOnHover: true
        },
        editNode: {
          state: true,
          fn: this.open_update_key,
          appearOnHover: true
        },
        deleteNode: {
          state: true,
          fn: this.delete_confirm,
          appearOnHover: true
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
  margin: 0;
  padding: 0;
}
body {
  background: #2d2d30;
}
@font-face {
  font-family: "icomoon";
  src: url(data:application/font-woff2;charset=utf-8;base64,d09GMgABAAAAAASIAA0AAAAACaAAAAQ0AAEAAAAAAAAAAAAAAAAAAAAAAAAAAAAAP0ZGVE0cGh4GYACCXhEICodwhlMLGAABNgIkAyYEIAWDGwd3GwQIUZQNzgKyHwc5WbzpxFx5C7kU0oGqT0h9Ps4xnqff739rn3OeSfqIRsjmoZiFSIh4qGaZTNLGn45ZqH/IROa66cB8ZYbYThUfPi97f6DMFGQ6BRVUTqea1rtOd51eV8IJ1NnX4v+/VznjZh/fm0IWCuH72S8/e+FaOLvwA1brPQXKt87JY1JIC1gtYw/tzOUlzC489kEI/HwROIQ/zyWj/NLBFCsoIsaioaDWz7mrR3lWPuAOOkj+2pUR2VWWaI/LWq+udquOV6xGAMftgkpujoi5jAzzlOYwxoaKIrav9br/XQeIbL9J93NdTxVjAFFJn1QgBYX6nwkN+o5I1xGYgBUsyQnyDtAAQShNT5VEqXDODfTucp11tpubl4MjrSRoHRfq5TqZHoai5IaFjCQSS+h4KNqtk+zsGF44epXKK4jH/Mggz/Hr1tmlsDzPMcKGA7rbZ8ew/MatxFscSIteugaFJ8m/TllYRjE3CSQpJ8d8lMphRI6RiS91I70mVE6+obpC+zDEQzquuVBiXr7U1RDZV6/UVPg3b9TE3OvX6XmSnSZOSVPJT0MsPdK8QbJjOFOzTrRnGc36KUeeVW+cdOD4xkjXhgvZq7fZd3Zs2u6//qKjsHEjmdhhtpM7HUR1R9eGc3aooDi1niKvv5BYSIecCCM3iK8oGkBx8obuSTeIQ3KGBDumqaOupD6xb41FvWUNVLcUGcR1RsbZP7X1hACEVRwUCwXm/qem/35yB6edjfl5RucwSyz5uppm2fp6yHCdYdIyfv/i2JmZW+aJWoPeNJGby3K5eatX6XvlalwKsXGOdm4FYJcAcAIX32GUAwxbGeneyPCuvLzoVmO+WUDrLipttCkQ6Q208JIva6z0qfSdP9/XTjY2+qFKCldSVBqzDlW+CCnOcM10a2lxy3TNKC42kIMCFEVJKsVZw92uD/0fjo5ohE1weIQiYLvkanz42b329+mZWdff+V3aFZ7qZnb3b0ECcEMF359SdyrK+5DZzgKhYPb0l68usycQQj4OfkQimcZJ21eZSL446xGZlUXDYyRxxJE4eThZO8IJGHtw+GTMW4Fu7IqjtRMG7p4Z4PWfzwECvNp39XWzUv4NJ82yejPdbNtZ32OogLAUDms3oGCz9KfAsAFyAShoHG8LPWoczYrj9gombuuBQu3NvRSoDiwDRoEdjArHGQ0uG3omruJ+Fjw8e2yjb6q0kZgDjAItJ4YcaIYCCgxKOKAFqwBWXFOHGGyDSTuzfmZmA2A6Jd7FPABU6SQY06fVMPxsjwqq7hCcOZ2GjTDNeyEKEaCBC/RdRkXQzci86PQoLwg6DXRi2WpUp47JNvN5oRBDaSkCeBKx6eqtVNhu/sOLewBkUJ+HAkhBRjnkVBAlURE10RCtSuibP9gdpR4bMNE07ShyE2kaosAI0RATjgU=)
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
input[type="text"],
input[type="password"] {
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
  position: relative;
}
.row_data::before {
  content: " ";
  border-top: 1px solid #3a3a3a;
  position: absolute;
  left: 0;
  bottom: -1px;
  right: 0;
  height: 1px;
  transform-origin: 0 0;
  transform: scaleY(0.7);
}
span {
  display: flex;
  align-items: center;
}

.row_data:hover {
  background: #363636;
}

.icon-closed:before {
  content: "\e903";
  font-size: 15px;
  color: #007acc;
}
.expanded:before {
  content: "\e904";
  font-size: 15px;
}
.add_icon:before {
  content: "\e900";
  font-size: 14px;

  color: #d7d9e8;
}
.edit_icon::before {
  content: "\e901";
  font-size: 14px;
  color: #d7d9e8;
}
.delete_icon::before {
  content: "\e902";
  font-size: 15px;
  color: #d7d9e8;
}
.folder_icon {
  display: flex;
  justify-content: center;
}
.folder_icon::before {
  content: "\e905";
  font-size: 15px;
  margin: 3px 2px 0 0;
}

.small-tree-indent {
  width: 16px;
  height: 35px;
  margin-right: 5px;
}

.icon-panel {
  margin-left: 20px;
}
.icon_parent {
  display: flex;
  align-items: center;
  margin-left: 10px;
}
.icon-expanded {
  margin-right: 5px;
}
.key {
  color: #979797;
  font-size: 15px;
}
.split {
  margin: 0 5px;
  font-weight: bold;
  color: #979797;
}
.text {
  color: #eee;
  font-size: 15px;
}
.content {
  display: flex;
  justify-content: space-between;
  width: 100%;
}
.icon-panel {
  margin-right: 10px;
}
.tree-indent {
  height: 35px;
}
.indents:last-child {
  border-left: 1px #eee solid;
}
.indents .tree-indent {
  position: relative;
}
.indents .tree-indent::before {
  content: " ";
  border-left: 1px dashed #eee;
  position: absolute;
  top: 0;
  bottom: 0;
  width: 1px;
  transform-origin: 0 0;
  transform: scaleX(0.5);
}
.indents .tree-indent:nth-child(1)::before {
  border-left: none;
}

header {
  height: 50px;
  background: #272727;
  display: flex;
  align-items: center;
  justify-content: space-between;
  color: #fff;
}
header span {
  margin-left: 15px;
  font-size: 20px;
}
</style>
