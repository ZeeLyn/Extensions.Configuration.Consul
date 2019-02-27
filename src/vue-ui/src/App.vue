<template>
  <div id="app">
    <header>
      <span>Consul configuration center</span>
      <div class="menu" v-if="logined"><span class="menu_icon"></span>
        <ul>
          <li v-on:click="changePassword"><a>Change password</a></li>
           <li v-on:click="logout"><a>Sign out</a></li>
        </ul>
      </div>
    </header>
    <div class="tool">
    <a class="add_icon new" v-on:click="open_new_key(null)">NEW</a>
    </div>
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
      <span style="color:#D93600;">{{new_key_error_message}}</span>
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
      title="Change password"
      ref="change_password"
      modal-theme="dark"
    >
      <div class="row">
        <span>Old password</span>
        <input type="password" minlength="6" maxlength="18" v-model="changePassword.oldPassword">
      </div>
      <div class="row">
        <span>New password</span>
        <input type="password" minlength="6" maxlength="18" v-model="changePassword.newPassword">
      </div>
      <div class="row">
        <span>Re-Enter</span>
        <input type="password" minlength="6" maxlength="18" v-model="changePassword.reEnter_password">
      </div>
      <span style="color:#D93600;">{{change_password_error_message}}</span>
      <a class="btn" slot="button" v-on:click="change_password_submit" color="light-grey">Save</a>
    </sweet-modal>

    <sweet-modal
      title="Sign in"
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
      <a class="btn" slot="button" v-on:click="login" color="light-grey">Sign in</a>
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
      logined:false,
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
      login_error_message: null,
      new_key_error_message:null,
      change_password_error_message:null
    };
  },
  methods: {
    EndWith: function(source, end) {
      var d = source.length - end.length;
      return d >= 0 && source.lastIndexOf(end) == d;
    },
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
           self.logined=true;
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
    open_new_key: function(node) {
      if(node)
        this.currentNode = node;
      else
        this.currentNode={
          type:0,
          id:""
        };
      this.$refs.create_key.open();
    },
    input_key: function(e) {
      if (!this.newkey) {
        this.isFolder = false;
        return;
      }
      this.isFolder = this.EndWith(this.newkey,"/");
      if(this.EndWith(this.newkey,":"))
        this.new_key_error_message="Key is not allowed to end with \" : \"";
      else
          this.new_key_error_message=null;
    },
    new_key_submit: function() {
      if(this.EndWith(this.newkey,":"))
        return;
      this.$refs.create_key.close();
      var key =
        this.currentNode.id +
        (this.currentNode.type != 0 && !this.EndWith(this.currentNode.id, ":")
          ? ":"
          : "") +
        this.newkey;
      this.$refs.loading.open();
      var self = this;
      axios
        .put(
          "http://localhost:5342/key/put",
          {
            key: key,
            value: this.newkey_value
          },
          {
            headers: {
              Authorization: "Bearer " + localStorage.getItem("access_token")
            }
          }
        )
        .then(res => {
          self.new_key_error_message=null;
          self.currentNode = null;
          (self.newkey = null), (self.newkey_value = null);
          self.load();
        })
        .catch(function(res) {
          if (err.response && err.response.status == 401) {
            self.check_auth();
          }else{
          self.new_key_error_message=res.response.data;
          self.$refs.create_key.open();
          }
        })
        .then(function() {
          self.$refs.loading.close();
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
          self.load();
        })
        .catch(function(res) {
          if (err.response && err.response.status == 401) {
            self.check_auth();
          }else
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
        .delete(
          "http://localhost:5342/key/delete/" +
            (node.nodes != undefined && node.nodes.length > 0 && node.type < 2
              ? "true"
              : "false"),
          {
            data: { key: node.id },
            headers: {
              Authorization: "Bearer " + localStorage.getItem("access_token")
            }
          }
        )
        .then(res => {
          self.load();
        })
        .catch(function(res) {
          if (err.response && err.response.status == 401) {
            self.check_auth();
          }else
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
            self.login_error_message=null;
            self.password=null;
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
    login: function() {
      var self = this;
      axios
        .post("http://localhost:5342/account/login", {
          password: self.password
        })
        .then(function(res) {
          self.$refs.login.close();
          localStorage.setItem("access_token", res.data);
          self.logined=true;
          self.load();
        })
        .catch(function(err) {
          if (err.response && err.response.data)
            self.login_error_message = err.response.data;
        })
        .then(function() {});
    },
    logout:function(){
      localStorage.removeItem("access_token");
      this.logined=false;
      this.check_auth();
    },
    changePassword:function(){
      this.changePassword={};
      this.$refs.change_password.open();
    },
    change_password_submit:function(){
      var self = this;
      this.$refs.loading.open();
      axios
        .put("http://localhost:5342/account/change.password", {
          oldPassword: self.changePassword.oldPassword,
          newPassword:self.changePassword.newPassword,
          reEnter: self.changePassword.reEnter_password
        })
        .then(function(res) {
          self.$refs.change_password.close();
          localStorage.setItem("access_token", res.data);
        })
        .catch(function(err) {
          if (err.response && err.response.data)
            self.change_password_error_message = err.response.data;
        })
        .then(function() {
          self.$refs.loading.close();
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
    font-family: 'icomoon';
    src: url(data:application/font-woff2;charset=utf-8;base64,d09GMgABAAAAAASoAA0AAAAACeQAAARSAAEAAAAAAAAAAAAAAAAAAAAAAAAAAAAAP0ZGVE0cGh4GYACCXhEICogkhnILGgABNgIkAygEIAWDGweBARsvCFGUT06S7COhur0TE2eynGUp4kkwhb/j4WutfL+7h3b3guzAoWOVigYSCsgBK1YExqCKy6qwRGGvooHHf8iz3h8oSTsgioG4G4zczqqzcrLr9LoSTqDOvg7+//1mRi6exR9midRo6X388TFPRA7JbGLxhzjRUAhp4k00iyZOXykrdTlu07ZvrnkT28VgY+RBCPi4b3kLX7Y+/6G+dGCEOSAJYSgUJOT8IW6oSA/Te1xnJomv1kZH9RbTHLgFTdZ53I9tyYBAAO7LEDK9KCFnLzq4p9jZAAZaSRKWTdb9v74Hmeu0/ufcShYGdBLQLWoi4wQJCfxngoL5jhB9byoYgXlgJsQ+iJeAAggEokoWSJLzOVdgg2xd3EoCA0O9fXT2doRH07iQtXWxYcHZMLbcQl6VSVHRv072XKd6evKS6ehVxvoK8rFIGpZEad06z1xBkkTetOGAi7rPkxekjVtJ2eJN3ax6DXaKQvJ1xiFaq2y9SSRNtrRFOcpYDyNxiY380kWjviayV94wXIP6YciHXITaQpV/+dJEkIVXrxBjld68QaSIr19n56lulpScLGMtz0Ku3dS1QfXkRVu/TvYSeP16q48koI2KtyhVZw5uuFC8/zavgf5N26PWX/QxbdxIUzssbnqnt4z6Bzec88QVZet6hl5/IWnUQ77E23LyK4Y4MKTccGHVG+TRtEKKb9FajwZQW+aUZk2btrlLCw0sKYBNuTaQ+WlG4YQ/LW1EANEEEcUNdhj/n1fw+8kdnPYzVJQb/BK1WPJstU4Q2tqQcJ3n8wt//xKF0dFblrktHDs0t6xMEMvKV69iuZWrcSne3c/o1wPQKgAEkX/EIBoAnBvHmv5Eo7G83NhjqLBL6DFi1uBeIWG5AxFqu6yjKbwpYv78CDfd0RFGmpA0IZOOonPtX8TXFAYUBXZ3BxYFFNbUWNjDEU7iLJOaokZzwMOoh7NmGpRLCWQmUmS57P748NO89vfp0bGA3xWDxhWeumjGdv82qQBuOCDip2rOxWUfMjZQaaocO/3lq/+YaiKij8MfkUUjOOnxqgjrL44FpxQX6xA8M2umD/kG+7r5wBdAggVyMuVvcNBxs4+bryaYduPZNMlsxpGfR4Qjr5H16vcDAUwfX/vZpc391g6LTfdm+rGyMr9AK4Fgo2xUzgIVi5U/BtoCiF0ACYUtZaH1hU0xR6F0RwVGLrAHif4TpngkQGhMQyBprEAga2xBoGjsRaAydAqBWuOOAAMLPEcWpROmELESzGNlWMUqsM1cmaOb1EVw8ZoD62xzUB+LqSwspsHBc+RdLNOA7QaYzDZFjxnwMz4maLpC4rnbgBlmGpodSi+ZDh7wq9Qn67rRW5Pjs4QyGTBtEPseswzob+81X6gqgLpKMnyrWHS/q1T1Wf4A4x6ARH+uEZGkyHKUqKKOJtroYlDvPzJ/2Kx3PH5sSKfTeZqyLJ0O9GAAI6RCWjQdAA==) format('woff2'),
        url(data:application/font-woff;charset=utf-8;base64,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) format('woff');
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
.menu_icon::before{
   content: "\e906";
  font-size: 20px;
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
.tool{ margin:10px 0; display: block;}
.new{ padding: 5px 15px 5px 5px; text-indent: 10px; border:1px #3a3a3a solid; display: inline-block; margin-left:15px; color: #fff; cursor: pointer;}
.new::before{margin-right: 5px; font-size:12px;}

.menu{position: relative; cursor: pointer; margin-right: 15px;}
.menu ul{display: none; position: absolute; top: 20px; right: 0; background: #686868;  width: 120px;}
.menu ul li{list-style: none; padding: 10px 10px;}
.menu ul li:hover{background: #fff;}
.menu ul li:hover a{color: #333;}
.menu ul li a{font-size: 12px; text-decoration: none; color: #2d2d30; display: block; height: 100%; widows: 100%;}
.menu:hover ul{display: block;}
</style>
