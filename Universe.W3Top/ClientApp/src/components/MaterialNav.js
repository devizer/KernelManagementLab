import React from 'react';
import PropTypes from 'prop-types';
import classNames from 'classnames';
import { withStyles } from '@material-ui/core/styles';
import Drawer from '@material-ui/core/Drawer';
import CssBaseline from '@material-ui/core/CssBaseline';
import AppBar from '@material-ui/core/AppBar';
import Toolbar from '@material-ui/core/Toolbar';
import List from '@material-ui/core/List';
import Typography from '@material-ui/core/Typography';
import Divider from '@material-ui/core/Divider';
import IconButton from '@material-ui/core/IconButton';
import MenuIcon from '@material-ui/icons/Menu';
import ChevronLeftIcon from '@material-ui/icons/ChevronLeft';
import ChevronRightIcon from '@material-ui/icons/ChevronRight';
import ListItem from '@material-ui/core/ListItem';
import ListItemIcon from '@material-ui/core/ListItemIcon';
import ListItemText from '@material-ui/core/ListItemText';
import InboxIcon from '@material-ui/icons/MoveToInbox';
import MailIcon from '@material-ui/icons/Mail';
import Icon from '@material-ui/core/Icon';
import StarIcon from '@material-ui/icons/Star';
import FlareIcon from '@material-ui/icons/Flare';
import InfoOutlinedIcon from '@material-ui/icons/InfoOutlined';
import AppGitInfo from "../AppGitInfo"
import { Collapse, Container, Navbar, NavbarBrand, NavbarToggler, NavItem, NavLink } from 'reactstrap';
import { Link } from 'react-router-dom';


const drawerWidth = 240;

const styles = theme => ({
    root: {
        display: 'flex',
    },
    appBar: {
        transition: theme.transitions.create(['margin', 'width'], {
            easing: theme.transitions.easing.sharp,
            duration: theme.transitions.duration.leavingScreen,
        }),
    },
    appBarShift: {
        width: `calc(100% - ${drawerWidth}px)`,
        marginLeft: drawerWidth,
        transition: theme.transitions.create(['margin', 'width'], {
            easing: theme.transitions.easing.easeOut,
            duration: theme.transitions.duration.enteringScreen,
        }),
    },
    menuButton: {
        marginLeft: 12,
        marginRight: 20,
    },
    hide: {
        display: 'none',
    },
    drawer: {
        width: drawerWidth,
        flexShrink: 0,
    },
    drawerPaper: {
        width: drawerWidth,
    },
    drawerHeader: {
        display: 'flex',
        alignItems: 'center',
        padding: '0 8px',
        ...theme.mixins.toolbar,
        justifyContent: 'flex-end',
    },
    drawerVer: {
        display: 'flex',
        alignItems: 'center',
        padding: '0 8px',
        ...theme.mixins.toolbar,
        justifyContent: 'flex-start',
    },
    content: {
        flexGrow: 1,
        padding: theme.spacing.unit * 3,
        transition: theme.transitions.create('margin', {
            easing: theme.transitions.easing.sharp,
            duration: theme.transitions.duration.leavingScreen,
        }),
        marginLeft: -drawerWidth,
    },
    contentShift: {
        transition: theme.transitions.create('margin', {
            easing: theme.transitions.easing.easeOut,
            duration: theme.transitions.duration.enteringScreen,
        }),
        marginLeft: 0,
    },
    
});

class PersistentDrawerLeft extends React.Component {
    state = {
        open: false,
    };

    handleDrawerOpen = () => {
        this.setState({ open: true });
    };

    handleDrawerClose = () => {
        this.setState({ open: false });
    };

    render() {
        const { classes, theme } = this.props;
        const { open } = this.state;

        const MainMenuLink = (text,to) => {
            return (
                <ListItem button component={NavLink} tag={Link} to={to} key={`${text}@MainMenu`} onClick={() => this.handleDrawerClose()}>
                    <ListItemIcon><FlareIcon/></ListItemIcon>
                    <ListItemText primary={text} />
                </ListItem>
            );
        };

        return (
            <div className={classes.root}>
                <CssBaseline />
                <AppBar
                    position="fixed"
                    className={classNames(classes.appBar, {
                        [classes.appBarShift]: open,
                    })}
                >
                    <Toolbar disableGutters={!open}>
                        <IconButton
                            color="inherit"
                            aria-label="Open drawer"
                            onClick={this.handleDrawerOpen}
                            className={classNames(classes.menuButton, open && classes.hide)}
                        >
                            <MenuIcon />
                        </IconButton>
                        <Typography variant="h6" color="inherit" noWrap>
                            W3 Top
                        </Typography>
                    </Toolbar>
                </AppBar>
                <Drawer
                    className={classes.drawer}
                    variant="persistent"
                    anchor="left"
                    open={open}
                    classes={{
                        paper: classes.drawerPaper,
                    }}
                >
                    <div className={classes.drawerHeader}>
                        <table border="0" cellPadding={0} cellSpacing={0} style={{width: "100%"}}><tr><td style={{textAlign: "left"}}>

                            <List>
                                    <ListItem button={false}>
                                        <ListItemIcon className={"version"}><InfoOutlinedIcon /></ListItemIcon>
                                        <ListItemText primary={"v" + AppGitInfo.Version} className={"version"} />
                                    </ListItem>
                            </List>

                            
                            {/*<small><InfoOutlinedIcon stye={{width: "9px", fontSize: 44}}/> v{AppGitInfo.Version}</small>*/}
                        </td><td width="24px">
                        <IconButton onClick={this.handleDrawerClose}>
                            {theme.direction === 'ltr' ? <ChevronLeftIcon /> : <ChevronRightIcon />}
                        </IconButton>
                        </td></tr></table>
                    </div>
                    <Divider />
                    <List>
                        {MainMenuLink("Disks", "/disk-v1")}
                        {MainMenuLink("Network-V2", "/net-v2")}
                        <Divider />
                        {MainMenuLink("Network", "/net-v1")}
                        <Divider />
                        {MainMenuLink("Single Y-axis chart", "/1-axis")}
                        {MainMenuLink("Double Y-axis one", "/2-axis")}
                    </List>
                    <Divider />
                </Drawer>
                <main
                    className={classNames(classes.content, {
                        [classes.contentShift]: open,
                    })}
                >
                    <div className={classes.drawerHeader} />
                    <Typography paragraph>
                        <FlareIcon /> Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor
                        incididunt ut labore et dolore magna aliqua. Rhoncus dolor purus non enim praesent
                        elementum facilisis leo vel. Risus at ultrices mi tempus imperdiet. Semper risus in
                        hendrerit gravida rutrum quisque non tellus. Convallis convallis tellus id interdum
                        velit laoreet id donec ultrices. Odio morbi quis commodo odio aenean sed adipiscing.
                        Amet nisl suscipit adipiscing bibendum est ultricies integer quis. Cursus euismod quis
                        viverra nibh cras. Metus vulputate eu scelerisque felis imperdiet proin fermentum leo.
                        Mauris commodo quis imperdiet massa tincidunt. Cras tincidunt lobortis feugiat vivamus
                        at augue. At augue eget arcu dictum varius duis at consectetur lorem. Velit sed
                        ullamcorper morbi tincidunt. Lorem donec massa sapien faucibus et molestie ac.
                    </Typography>
                </main>
            </div>
        );
    }
}

PersistentDrawerLeft.propTypes = {
    classes: PropTypes.object.isRequired,
    theme: PropTypes.object.isRequired,
};

export default withStyles(styles, { withTheme: true })(PersistentDrawerLeft);
