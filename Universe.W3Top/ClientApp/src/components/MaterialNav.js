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
import ListSubheader from '@material-ui/core/ListSubheader';
import InboxIcon from '@material-ui/icons/MoveToInbox';
import MailIcon from '@material-ui/icons/Mail';
import Icon from '@material-ui/core/Icon';
import StarIcon from '@material-ui/icons/Star';
import FlareIcon from '@material-ui/icons/Flare';
import ScoreIcon from '@material-ui/icons/Score';
import InsertChartOutlinedIcon from '@material-ui/icons/InsertChartOutlined';
import InfoOutlinedIcon from '@material-ui/icons/InfoOutlined';
import { Collapse, Container, Navbar, NavbarBrand, NavbarToggler, NavItem, NavLink } from 'reactstrap';
import { Link } from 'react-router-dom';

import { faServer } from '@fortawesome/free-solid-svg-icons'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { ReactComponent as HardDiskIcon } from '../icons/hard-disk.svg';

import AppGitInfo from "../AppGitInfo"
import dataSourceStore from "../stores/DataSourceStore";
import * as Helper from "../Helper";

import { ReactComponent as DisksIconSvg } from '../icons/Disks-Icon.svg';
const DisksIcon = (size=24,color='#333') => (<DisksIconSvg style={{width: size,height:size,fill:color,strokeWidth:'6px',stroke:color }} />);



const drawerWidth = 250;

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
        paddingBottom: theme.spacing.unit,
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
    
    constructor(props) {
        super(props);
        
        this.updateBrief = this.updateBrief.bind(this);
        this.updateGlobalDataSource = this.updateGlobalDataSource.bind(this);
    }
    

    componentDidMount() {
        let x1 = dataSourceStore.on('storeUpdated', this.updateGlobalDataSource);
        let x2 = dataSourceStore.on('briefUpdated', this.updateBrief);
    }
    
    // It is never called, but 
    componentWillUnmount() {
        let x1 = dataSourceStore.removeListener('storeUpdated', this.updateGlobalDataSource);
        let x2 = dataSourceStore.removeListener('briefUpdated', this.updateBrief);
    }


    handleDrawerOpen = () => {
        this.setState({ open: true });
    };

    handleDrawerClose = () => {
        this.setState({ open: false });
    };
    
    updateGlobalDataSource()
    {
        Helper.log("DATASOURCE UPDATED handler AT MaterialNav");
        let system = dataSourceStore.getDataSource().system;
        if (Helper.Common.objectIsNotEmpty(system))
            this.setState({system: system});
    }

    updateBrief()
    {
        console.log("BRIEF UPDATED handler AT MaterialNav");
        this.setState({system: dataSourceStore.getBriefInfo().system});
    }
    
    sis = {
      def: {
          textAlign: "right",
          paddingRight: 12,
          paddingBottom: 10,
          verticalAlign: "top",
          color: "#888",
          borderBottom: "1px dotted white",
      },
      val: {
          fontWeight: "normal",
          verticalAlign: "top",
          paddingBottom: 10,
      },
      notReady: {
          borderBottom: "1px dotted grey",
          width: 240,
          display: "inline-block", 
          paddingBottom: 0
      },
        ready: {
            borderBottom: "1px dotted white",
            display: "inline-block",
            paddingBottom: 0
        }

    };
    
    

    render() {
        const { classes, theme } = this.props;
        const { open } = this.state;
        
        const asHtml = (raw) => <span dangerouslySetInnerHTML={{__html: raw}} />;
        const infoAsComponent = (info) => asHtml(Helper.Common.formatInfoHeader(info));
        const SysValueNotReady = () => (<span style={this.sis.notReady}>&nbsp;</span>);
        let sysInfoValueId = 0;
        const SysRow = (def,value) => { return (
            <tr>
                <td style={this.sis.def}>{def}</td>
                <td style={this.sis.val}>{ value ? <span style={this.sis.ready} id={`COMMON_INFO_HEADER_` + (++sysInfoValueId)}>{value}</span> : <SysValueNotReady/>}</td>
            </tr>
        )};

        let idMainLink = 0;
        const MainMenuLink = (icon, text, routeTo) => {
            return (
                <ListItem id={`THE_MENU_${++idMainLink}`} button component={NavLink} tag={Link} to={routeTo} key={`${text}@MainMenu`} onClick={() => this.handleDrawerClose()}>
                    <ListItemIcon>{icon}</ListItemIcon>
                    <ListItemText id={`THE_MENU_LINK_${idMainLink}`} primary={text} />
                </ListItem>
            );
        };

        let system = this.state.system;
        if (!Helper.Common.objectIsNotEmpty(system)) system = {};
        // let hostname = Helper.Common.tryGetProperty(dataSourceStore.getDataSource(), "hostname"); 
        // let [hasSystem, system] = Helper.Common.tryGetProperty(dataSourceStore.getDataSource(), "system");
        // if (!hasSystem) system = {};
        // let hostname = Helper.System.getHostName(dataSourceStore.getDataSource());
        
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
                        <IconButton id={"APP_SYSTEM_ICON"}
                            color="inherit"
                            aria-label="Show menu"
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
                        <table border="0" cellPadding={0} cellSpacing={0} style={{width: "100%"}}><tbody><tr><td style={{textAlign: "left"}}>

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
                        </td></tr></tbody></table>
                    </div>
                    <Divider />
                    <List>
                        {MainMenuLink(<InsertChartOutlinedIcon/>,"Network Live Chart", "/net-v2")}
                        <Divider />
                        {/*{MainMenuLink(<FontAwesomeIcon icon={faServer} style={{marginLeft:4, marginRight:5}}/>,"Live Mounts", "/mounts")}*/}
                        {/*{MainMenuLink(<HardDiskIcon style={{width:20,height:20,marginLeft:2,marginRight:2,opacity:0.6}}/>,"Live Mounts", "/mounts")}*/}
                        {MainMenuLink(DisksIcon(24,'#333'),"Live Mounts", "/mounts")}
                        
                        
                        {MainMenuLink(<InsertChartOutlinedIcon/>,"Disks Live Chart", "/disks")}
                        {MainMenuLink(<ScoreIcon/>,"Disk Benchmark", "/disk-benchmark")}
                        <Divider />
                        <ListSubheader disableSticky={true} style={{paddingTop:32, paddingBottom:16, textAlign: "center", lineHeight: "1rem"}}>SAND-BOX<br/>please ignore</ListSubheader>
                        {MainMenuLink(<FlareIcon/>,"Missed network", "/net-v1")}
                        <Divider />
                        {MainMenuLink(<FlareIcon/>,"Single Y-axis chart", "/1-axis")}
                        {MainMenuLink(<FlareIcon/>,"Double Y-axis one", "/2-axis")}
                    </List>
                    <Divider />
                </Drawer>
                <main
                    className={classNames(classes.content, {
                        [classes.contentShift]: open,
                    })}
                >
                    <div className={classes.drawerHeader} />

                    <table border="0" cellSpacing="0" cellPadding="0"><tbody>
                        {SysRow("host", system.hostname)}
                        {SysRow("os", infoAsComponent(system.os))}
                        {SysRow("cpu", infoAsComponent(system.processor))}
                        {SysRow("ram", system.memory)}
                    </tbody></table>
                    
                    <Typography paragraph className={classes.hide}>
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
