import MuiThemeProvider from 'material-ui/styles/MuiThemeProvider';
import AppBar from 'material-ui/AppBar';
import RaisedButton from 'material-ui/RaisedButton';
import TextField from 'material-ui/TextField';
import React from 'react';
import Axios from 'axios';

class Login extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            username: '',
            password: ''
        };
    }

    handleClick(event){
        const apiBaseUrl = "http://localhost:3000/api/";
        let self = this;
        const payload={
            "email": this.state.username,
            "password": this.state.password
        };
        Axios.post(apiBaseUrl + 'login', payload)
            .then(function (response) {
                console.log(response);

                if(response.data.code == 200){
                    console.log("Login successfull");
                    let uploadScreen=[];
                    uploadScreen.push(<UploadScreen appContext={self.props.appContext}/>);
                    self.props.appContext.setState({loginPage:[],uploadScreen:uploadScreen});
                }
                else if(response.data.code == 204){
                    console.log("Username password do not match");
                    alert("username password do not match");
                }
                else{
                    console.log("Username does not exists");
                    alert("Username does not exist");
                }
            })
            .catch(function (error) {
                console.log(error);
            });
        }

    render() {
        return (
            <div>
                <MuiThemeProvider>
                    <div>
                        <AppBar title="Login"/>
                        <TextField 
                            hintText="Enter your username" 
                            floatingLabelText="Username"
                            onChange={function (event, newValue) 
                                {this.setState({username: newValue});}}
                        />
                        <br/>
                        <TextField 
                            type="password"
                            hintText="Enter your password"
                            floatingLabelText="Password"
                            onChange={function (event, newValue) 
                                {this.setState({password: newValue});}}
                        />
                        <br/>
                        <RaisedButton 
                            label="Submit"
                            primary
                            style={style}
                            onClick={function(event) 
                                {this.handleClick(event);}}
                        />
                    </div>
                </MuiThemeProvider>
            </div>
        );
    }
}

const style ={
    margin: 15
};

export default Login;